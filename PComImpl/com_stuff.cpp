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
UINT_PTR __stdcall __shouldHookFunction( FunctionID functionId, BOOL * shouldHook );

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

	template< typename T >
	bool StringStartsWith( std::basic_string< T > const& s, std::basic_string< T > const& prefix )
	{
		std::basic_string<T>::const_iterator a = s.begin();
		std::basic_string<T>::const_iterator b = prefix.begin();

		while( b != prefix.end() )
			if (*a++ != *b++)
				return false;

		return true;
	}

	template< typename T >
	bool StringStartsWithAny( std::basic_string< T > const& s, 
		std::basic_string< T > const * begin, 
		std::basic_string< T > const * end )
	{
		std::basic_string< T > const * p = begin;
		while( p != end )
			if (StringStartsWith( s, *p++ ))
				return true;

		return false;
	}
}

class Profiler : public ProfilerBase
{
	ProfileWriter writer;
	std::map< UINT, bool > hooked;
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

		if( hooked.find( functionId ) != hooked.end() )
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

	void OnFunctionEnter( UINT functionId, UINT_PTR interesting )
	{
		bool parentInteresting = fns.EmptyOrPeek();// fns.Empty() || fns.Peek();
		bool functionInteresting = interesting == functionId; //interesting[ functionId ];

		fns.Push(functionInteresting);

		if (parentInteresting || functionInteresting)
			writer.WriteEnterFunction( functionId, GetCurrentThreadID() );
	}

	void OnFunctionLeave( UINT functionId )
	{
		//bool functionInteresting = fns.Pop();
		//bool parentInteresting = fns.EmptyOrPeek();// fns.Empty() || fns.Peek();

		//if (parentInteresting || functionInteresting)
		if( fns.PopOrEmptyOrPeek() )
			writer.WriteLeaveFunction( functionId, GetCurrentThreadID() );
	}

	void OnFunctionTail( UINT functionId )
	{
		//bool functionInteresting = fns.Pop();
		//bool parentInteresting = fns.EmptyOrPeek();// fns.Empty() || fns.Peek();

		//if (parentInteresting || functionInteresting)
		if( fns.PopOrEmptyOrPeek() )
			writer.WriteTailFunction( functionId, GetCurrentThreadID() );
	}

	bool IsPrivate( DWORD flags )
	{
		flags &= mdMemberAccessMask;
		return (flags == mdPrivateScope || flags == mdPrivate || flags == mdFamANDAssem || flags == mdAssem);
	}

	std::wstring GetFunctionName(UINT functionId, bool& isPrivate )
	{
		mdToken methodToken, classToken, newClassToken;
		IMetaDataImport * pm = NULL;

		if (FAILED(profiler->GetTokenAndMetaDataFromFunction( functionId, IID_IMetaDataImport, (IUnknown **) &pm, &methodToken )))
			return L"<no metadata>";

		wchar_t buf[512];
		DWORD size = 512;
		DWORD flags = 0;
		if (FAILED(pm->GetMethodProps( methodToken, &classToken, buf, size, &size, &flags, NULL, NULL, NULL, NULL )))
		{
			pm->Release();
			return L"<no metadata>";
		}

		isPrivate = IsPrivate( flags );

		std::wstring class_buf_string;
		while( true )
		{
			wchar_t class_buf[512];
			size = 512;
			if (FAILED(pm->GetTypeDefProps( classToken, class_buf, size, &size, NULL, NULL )))
			{
				pm->Release();
				return L"<no metadata>";
			}
			class_buf_string = std::wstring( class_buf ) + class_buf_string;

			if( FAILED( pm->GetNestedClassProps( classToken, &newClassToken ) ) )
				break;
			if( !newClassToken || newClassToken == classToken )
				break;

			class_buf_string = L"$" + class_buf_string;

			classToken = newClassToken;
		}

		pm->Release();

		return class_buf_string + L"::" + std::wstring( buf );
	}

	bool FunctionIsTrivial( UINT functionId )
	{
		ClassID c;
		mdToken md;
		ModuleID module;
		if( !SUCCEEDED( profiler->GetFunctionInfo( functionId, &c, &module, &md ) ) )
			return false;
		BYTE const* ilPtr;
		ULONG ilSize;
		if( !SUCCEEDED( profiler->GetILFunctionBody( module, md, &ilPtr, &ilSize ) ) )
			return false;

		ULONG bodySize = 0;
		BYTE const* body = ilPtr + 1;
		if( ( ilPtr[0] & 2 ) == 2 )
			bodySize = ilPtr[0] >> 2;
		else
		{
			bodySize = *(ULONG*)(ilPtr + 4);
			body = ilPtr + 12;
		}

		// is ldarg.0; ldfld [token]; ret ?
		if( bodySize >= 7 && body[0] == 0x2 && body[1] == 0x7b && body[6] == 0x2a )
			return true;

		return false;
	}

	UINT_PTR ShouldHookFunction( FunctionID functionId, BOOL * shouldHook )
	{
		bool isPrivate = false;
		char sz[1024];
		std::wstring ws = GetFunctionName( functionId, isPrivate );
		sprintf(sz, "0x%08x=%ws", functionId, ws.c_str());
		Log(sz);

		static std::wstring str[] = { L"System.", L"Microsoft.", L"MS." };
		//interesting[ functionId ] = !StringStartsWithAny( ws, str, str + 3 );
		//return true;
		bool isInteresting = !StringStartsWithAny( ws, str, str + 3 );

		if( !FunctionIsTrivial( functionId ) && ( isInteresting || !isPrivate ) )
		{
			*shouldHook = true;
			hooked[functionId] = true;
			return isInteresting ? functionId : functionId + 1;
		}
		*shouldHook = false;
		return functionId;
	}
};

UINT_PTR __stdcall __shouldHookFunction( FunctionID functionId, BOOL * shouldHook )
{
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
