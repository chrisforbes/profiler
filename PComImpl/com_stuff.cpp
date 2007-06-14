#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cor.h>
#include <corprof.h>
#include <cstdio>
#include <string>
#include <map>

#include "classfactory.h"
#include "thread_ptr.h"

#pragma comment(lib, "corguids.lib")

extern const GUID __declspec( selectany ) CLSID_PROFILER = { 0xc1e9fe1f, 0xf517, 0x45c0, { 0xbb, 0x0e, 0xef, 0xae, 0xcc, 0x94, 0x01, 0xfc }};

#include "profiler_base.h"
#include "profilewriter.h"

class Profiler * __inst;

void __funcEnter( UINT functionId );
void __funcLeave( UINT functionId );
void __funcTail( UINT functionId );

class Profiler : public ProfilerBase
{
	std::map<UINT, bool> seen_function;
	thread_ptr<UINT> thread;

	ProfileWriter writer;

public:
	Profiler()
		: ProfilerBase( COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_MONITOR_THREADS ), 
		writer( "c:\\profile.bin" )
	{
		__inst = this;
	}

	STDMETHOD(Initialize)( IUnknown * pCorProfilerInfoUnk )
	{
		ProfilerBase::Initialize( pCorProfilerInfoUnk );
		profiler->SetEnterLeaveFunctionHooks( __funcEnter, __funcLeave, __funcTail );
		writer.WriteClockFrequency();
		return S_OK;
	}

	volatile UINT lastSeenThread;

	void ReportThreadTransition()
	{
		UINT currentManagedThread = *thread.get();
		if (currentManagedThread != InterlockedExchange( (LONG volatile *)&lastSeenThread, currentManagedThread ))
			writer.WriteThreadTransition( currentManagedThread );
	}

	void OnFunctionEnter( UINT functionId )
	{
		ReportThreadTransition();

		if (!seen_function[functionId])
		{
			char sz[1024];
			std::wstring ws = GetFunctionName( functionId );
			sprintf(sz, "0x%08x=%ws", functionId, ws.c_str());
			Log(sz);

			writer.WriteFunctionBinding( functionId, ws );
			seen_function[functionId] = true;
		}
		writer.WriteEnterFunction( functionId );
	}

	void OnFunctionLeave( UINT functionId )
	{
		ReportThreadTransition();
		writer.WriteLeaveFunction( functionId );
	}

	void OnFunctionTail( UINT functionId )
	{
		ReportThreadTransition();
		writer.WriteTailFunction( functionId );
	}

	STDMETHOD(ThreadAssignedToOSThread)(UINT managed, DWORD os)
	{
		if (NULL == thread.get())
		{
			UINT * threadinfo = new UINT;
			thread.set( threadinfo );
		}

		*thread.get() = managed;

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

void __declspec(naked) __funcEnter ( UINT functionId )
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
		ret 4
	}
}

void __declspec(naked) __funcLeave ( UINT functionId )
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
		ret 4
	}
}

void __declspec(naked) __funcTail ( UINT functionId )
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
		ret 4
	}
}

STDAPI DllGetClassObject( IID const & rclsid, IID const & riid, void ** ppv )
{
	static ClassFactory<Profiler> classFactory;

	return (CLSID_PROFILER == rclsid) ? 
		classFactory.QueryInterface( riid, ppv ) : E_OUTOFMEMORY;
}
