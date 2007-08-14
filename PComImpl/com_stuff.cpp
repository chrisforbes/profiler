#define WIN32_LEAN_AND_MEAN
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
	std::map< UINT, bool > interesting;
	FunctionStack fns;

public:
	Profiler()
		: ProfilerBase( COR_PRF_MONITOR_ENTERLEAVE, GetEnv( txtProfileEnv ) ), 
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
		return S_OK;
	}

	STDMETHOD(Shutdown)()
	{
		writer.Flush();
		return S_OK;
	}

	void OnFunctionEnter( UINT functionId )
	{
		bool parentInteresting = fns.Empty() || fns.Peek();
		bool functionInteresting = interesting[ functionId ];

		fns.Push(functionInteresting);

		if (parentInteresting || functionInteresting)
		{
			LogEx( "-- %d %d\n", parentInteresting, functionInteresting );
			writer.WriteEnterFunction( functionId, profiler );
		}
	}

	void OnFunctionLeave( UINT functionId )
	{
		bool functionInteresting = fns.Pop();
		bool parentInteresting = fns.Empty() || fns.Peek();

		if (parentInteresting || functionInteresting)
		{
			LogEx( "-- %d %d\n", parentInteresting, functionInteresting );
			writer.WriteLeaveFunction( functionId, profiler );
		}
	}

	void OnFunctionTail( UINT functionId )
	{
		bool functionInteresting = fns.Pop();
		bool parentInteresting = fns.Empty() || fns.Peek();

		if (parentInteresting || functionInteresting)
		{
			LogEx( "-- %d %d\n", parentInteresting, functionInteresting );
			writer.WriteTailFunction( functionId, profiler );
		}
	}

	bool IsPrivate( DWORD flags )
	{
		flags &= mdMemberAccessMask;
		return (flags == mdPrivateScope || flags == mdPrivate || flags == mdFamANDAssem || flags == mdAssem);
	}

	std::wstring GetFunctionName( UINT functionId, bool& isPrivate )
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

	bool ShouldHookFunction( UINT functionId )
	{
		bool isPrivate;
		char sz[1024];
		std::wstring ws = GetFunctionName( functionId, isPrivate );
		sprintf(sz, "0x%08x=%ws", functionId, ws.c_str());
		Log(sz);

		static std::wstring str[] = { L"System.", L"Microsoft.", L"MS." };
		interesting[ functionId ] = !StringStartsWithAny( ws, str, str + 3 );
		return true;
	}
};

UINT_PTR __stdcall __shouldHookFunction( UINT functionId, BOOL * shouldHook )
{
	*shouldHook = __inst->ShouldHookFunction( functionId );
	return functionId;
}

void __declspec(naked) __funcEnter ( UINT functionId, UINT_PTR, COR_PRF_FRAME_INFO, COR_PRF_FUNCTION_ARGUMENT_INFO* )
{
	__asm
	{
		push eax
		push ecx
		push edx
		push [esp+16]
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
