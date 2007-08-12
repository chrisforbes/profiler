#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cor.h>
#include <corprof.h>
#include <cstdio>
#include <string>
#include <map>

#include <corhdr.h>

#include "classfactory.h"

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

	void OnFunctionEnter( UINT functionId )
	{
		writer.WriteEnterFunction( functionId, profiler );
	}

	void OnFunctionLeave( UINT functionId )
	{
		writer.WriteLeaveFunction( functionId, profiler );
	}

	void OnFunctionTail( UINT functionId )
	{
		writer.WriteTailFunction( functionId, profiler );
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
			if( newClassToken == classToken || newClassToken == 0 )
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

		if (isPrivate)
		{
			sprintf(sz, "--- %ws is private", ws.c_str());
			Log(sz);
		}

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
