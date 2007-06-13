#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cor.h>
#include <corprof.h>
#include <cstdio>
#include <string>
#include <map>

#include "classfactory.h"

#pragma comment(lib, "corguids.lib")

extern const GUID __declspec( selectany ) CLSID_PROFILER = { 0xc1e9fe1f, 0xf517, 0x45c0, { 0xbb, 0x0e, 0xef, 0xae, 0xcc, 0x94, 0x01, 0xfc }};

#include "profiler_base.h"

class Profiler * __inst;

class Profiler : public ProfilerBase
{
public:
	Profiler()
		: ProfilerBase( COR_PRF_MONITOR_ENTERLEAVE )
	{
		__inst = this;
	}

	STDMETHOD(Initialize)( IUnknown * pCorProfilerInfoUnk )
	{
		ProfilerBase::Initialize( pCorProfilerInfoUnk );
		profiler->SetEnterLeaveFunctionHooks( Profiler::DispatchFunctionEnter, 0, 0 );
		return S_OK;
	}

	static void DispatchFunctionEnter( UINT functionId )
	{
		__inst->OnFunctionEnter( functionId );
	}

	std::map<UINT, UINT> seen_function;

	void OnFunctionEnter( UINT functionId )
	{
		if (!(seen_function[functionId]++))
		{
			char sz[1024];
			std::wstring ws = GetFunctionName( functionId );
			sprintf(sz, "0x%08x=%ws", functionId, ws.c_str());
			Log(sz);
		}
	}

	STDMETHOD(Shutdown)()
	{
		char sz[64];
		for( std::map<UINT, UINT>::const_iterator i = seen_function.begin(); i != seen_function.end(); i++ )
		{
			sprintf(sz, "0x%08x=%d", i->first, i->second);
			Log(sz);
		}

		return S_OK;
	}

	std::wstring GetFunctionName( UINT functionId )
	{
		mdToken methodToken, classToken;
		IMetaDataImport * pm = NULL;

		if (FAILED(profiler->GetTokenAndMetaDataFromFunction( functionId, IID_IMetaDataImport, (IUnknown **) &pm, &methodToken )))
			return L"<no metadata>";

		wchar_t buf[512];
		DWORD size = 512;
		if (FAILED(pm->GetMethodProps( methodToken, &classToken, buf, size, &size, NULL, NULL, NULL, NULL, NULL )))
		{
			pm->Release();
			return L"<no metadata>";
		}

		wchar_t class_buf[512];
		size = 512;
		if (FAILED(pm->GetTypeDefProps( classToken, class_buf, size, &size, NULL, NULL )))
		{
			pm->Release();
			return L"<no metadata>";
		}

		pm->Release();

		return std::wstring( class_buf ) + L"::" + std::wstring( buf );
	}
};

STDAPI DllGetClassObject( IID const & rclsid, IID const & riid, void ** ppv )
{
	static ClassFactory<Profiler> classFactory;

	return (CLSID_PROFILER == rclsid) ? 
		classFactory.QueryInterface( riid, ppv ) : E_OUTOFMEMORY;
}
