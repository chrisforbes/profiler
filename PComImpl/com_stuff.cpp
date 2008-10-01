#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <cor.h>
#include <corprof.h>
#include <cstdio>
#include <string>
#include <map>
#include <stack>

#include <corhdr.h>

#include "classfactory.h"

#include "thread_ptr.h"
#include "function_stack.h"

#pragma comment(lib, "corguids.lib")

extern const GUID __declspec( selectany ) CLSID_PROFILER = 
	{ 0xc1e9fe1f, 0xf517, 0x45c0, { 0xbb, 0x0e, 0xef, 0xae, 0xcc, 0x94, 0x01, 0xfc }};

#include "profiler_base.h"
#include "profilewriter.h"

class Profiler * __inst;

void __funcEnter( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO, COR_PRF_FUNCTION_ARGUMENT_INFO* );
void __funcLeave( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO, COR_PRF_FUNCTION_ARGUMENT_RANGE* );
void __funcTail( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO );
UINT_PTR __stdcall __shouldHookFunction( UINT functionId, BOOL * shouldHook );

char const * txtProfileEnv = "ijwprof_txt";
char const * binProfileEnv = "ijwprof_bin";

namespace
{
	std::string GetEnv( std::string const & name )
	{
		static char buf[MAX_PATH];
		GetEnvironmentVariableA( name.c_str(), buf, MAX_PATH );
		return buf;
	}
}

class Profiler : public ProfilerBase
{
	ProfileWriter writer;
	FunctionStack fns;
	ThreadPtr< UINT > unwindFunc;

public:
	Profiler()
		: ProfilerBase( COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_MONITOR_EXCEPTIONS, GetEnv( txtProfileEnv ) ), 
		writer( GetEnv( binProfileEnv ) )
	{
		__inst = this;
	}

	STDMETHOD(Initialize)( IUnknown * pCorProfilerInfoUnk )
	{
		HRESULT hr;
		if (FAILED(hr = ProfilerBase::Initialize( pCorProfilerInfoUnk )))
		{
			Log("Failed to initialize");
			return hr;
		}
		profiler->SetEnterLeaveFunctionHooks2( __funcEnter, __funcLeave, __funcTail );
		profiler->SetFunctionIDMapper( __shouldHookFunction );
		writer.WriteClockFrequency();
		writer.WriteTimeBase();
		return S_OK;
	}

	STDMETHOD(Shutdown)()
	{
		writer.Flush();
		return S_OK;
	}

	STDMETHOD(ExceptionUnwindFunctionEnter)(UINT functionId)
	{
		unwindFunc.set( (UINT*) functionId );
		return S_OK;
	}

	STDMETHOD(ExceptionUnwindFunctionLeave)()
	{
		UINT functionId = (UINT)unwindFunc.get();
		if (!functionId)
		{
			Log("Bogus ExceptionUnwindFunctionLeave");
			return S_OK;	//pretend it works, dont break the CLR :)
		}

		OnFunctionLeave( functionId );
		unwindFunc.set(0);
		return S_OK;
	}

	unsigned int GetCurrentThreadID()
	{
		unsigned int result;
		profiler->GetCurrentThreadID( &result );
		return result;
	}

	void OnFunctionEnter( UINT functionId, UINT interesting )
	{
		bool functionInteresting = interesting == 1;
		bool parentInteresting = fns.EmptyOrPeekThenPush(functionInteresting);

		if (parentInteresting || functionInteresting)
			writer.WriteEnterFunction( functionId, GetCurrentThreadID() );
	}

	void OnFunctionLeave( UINT functionId )
	{
		if( fns.PopOrEmptyOrPeek() )
			writer.WriteLeaveFunction( functionId, GetCurrentThreadID() );
	}

	void OnFunctionTail( UINT functionId )
	{
		if( fns.PopOrEmptyOrPeek() )
			writer.WriteTailFunction( functionId, GetCurrentThreadID() );
	}

	bool IsPrivate( DWORD flags )
	{
		flags &= mdMemberAccessMask;
		return (flags == mdPrivateScope || flags == mdPrivate || flags == mdFamANDAssem || flags == mdAssem);
	}

	// does simplistic ucs-2 -> 8859-1 conversion while copying.
	static __forceinline char * append( char * dest, wchar_t const * src, size_t n )
	{
		while(n--)
			*dest++ = (char)(unsigned char)*src++;

		return dest;
	}

	// returns ptr to start of toplevel ns
	char * GetFunctionName(UINT functionId, bool& isPrivate, char * dest )
	{
		mdToken methodToken, classToken;
		IMetaDataImport * pm = NULL;
		wchar_t temp[512];
		DWORD size = 512;

		if (FAILED(profiler->GetTokenAndMetaDataFromFunction( functionId, IID_IMetaDataImport, (IUnknown **) &pm, &methodToken )))
		{
			*dest = 0;
			return 0;
		}

		DWORD flags = 0;
		if (FAILED(pm->GetMethodProps( methodToken, &classToken, temp, size, &size, &flags, NULL, NULL, NULL, NULL )))
		{
			pm->Release();
			*dest = 0;
			return 0;
		}

		dest = append( dest, temp, wcslen(temp) );
		*dest++ = '@';

		isPrivate = IsPrivate( flags );

		char * result = dest;

		while( true )
		{
			size = 512;
			if (FAILED(pm->GetTypeDefProps( classToken, temp, size, &size, NULL, NULL )))
			{
				pm->Release();
				*dest = 0;
				return 0;
			}

			dest = append( dest, temp, wcslen(temp) );

			mdToken newClassToken;
			if( FAILED( pm->GetNestedClassProps( classToken, &newClassToken ) ) )
				break;
			if( !newClassToken || newClassToken == classToken )
				break;

			*dest++ = '@';
			result = dest;

			classToken = newClassToken;
		}

		pm->Release();

		*dest = 0;
		return result;
	}

#define match( x, y )\
	(0 == memcmp( x, y, sizeof(y) - 1 ))

	UINT_PTR ShouldHookFunction( UINT functionId, BOOL * shouldHook )
	{
		bool isPrivate = false;
		char sz[4096];
		char * psz = sz + sprintf( sz, "0x%08x=", functionId );
		psz = GetFunctionName( functionId, isPrivate, psz );
		Log(sz);

		bool isInteresting = true;
		
		if (psz)
		{
			if (match( psz, "System." )) isInteresting = false;
			if (match( psz, "Microsoft." )) isInteresting = false;
			if (match( psz, "MS." )) isInteresting = false;
		}

		if( isInteresting || !isPrivate )
		{
			*shouldHook = true;
			return isInteresting ? (UINT_PTR)1 : (UINT_PTR)2;
		}
		return NULL;
	}
};

UINT_PTR __stdcall __shouldHookFunction( UINT functionId, BOOL * shouldHook )
{
	*shouldHook = false;
	return __inst->ShouldHookFunction( functionId, shouldHook );
}

void __declspec(naked) __funcEnter ( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO, COR_PRF_FUNCTION_ARGUMENT_INFO* )
{
	__asm
	{
		push eax
		push ecx
		push edx
		push [esp+20]
		push [esp+20]
		mov ecx, __inst
		call Profiler::OnFunctionEnter
		pop edx
		pop ecx
		pop eax
		ret 16
	}
}

void __declspec(naked) __funcLeave ( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO, COR_PRF_FUNCTION_ARGUMENT_RANGE* )
{
	__asm
	{
		push eax
		push ecx
		push edx
		push [esp+16]
		mov ecx, __inst
		call Profiler::OnFunctionLeave
		pop edx
		pop ecx
		pop eax
		ret 16
	}
}

void __declspec(naked) __funcTail ( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO )
{
	__asm
	{
		push eax
		push ecx
		push edx
		push [esp+16]
		mov ecx, __inst
		call Profiler::OnFunctionTail
		pop edx
		pop ecx
		pop eax
		ret 12
	}
}

STDAPI DllGetClassObject( IID const & rclsid, IID const & riid, void ** ppv )
{
	static ClassFactory<Profiler> classFactory;

	return (CLSID_PROFILER == rclsid) ? 
		classFactory.QueryInterface( riid, ppv ) : E_OUTOFMEMORY;
}
