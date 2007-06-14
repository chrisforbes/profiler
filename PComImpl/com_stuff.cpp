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

#define BUFFER_SIZE		512 * 1024

class ProfileWriter
{
	HANDLE fh;
	unsigned char * buffer;
	unsigned char * cur;
	CRITICAL_SECTION cs;

public:
	ProfileWriter( std::string const & filename )
	{
		fh = CreateFileA( filename.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, 0 );
		buffer = new unsigned char[ BUFFER_SIZE ];
		InitializeCriticalSection( &cs );
	}

	~ProfileWriter()
	{
		Flush();
		CloseHandle( fh );
		delete[] buffer;
		DeleteCriticalSection( &cs );
	}

	void WriteData( unsigned char * data, unsigned int length )
	{
		EnterCriticalSection( &cs );

		DWORD b;
		if (cur + length >= buffer + BUFFER_SIZE)
		{
			WriteFile( fh, buffer, cur - buffer, &b, NULL );
			cur = buffer;
		}

		memcpy( cur, data, length );
		cur += length;

		LeaveCriticalSection( &cs );
	}

	void Flush()
	{
		EnterCriticalSection( &cs );

		if (cur != buffer)
		{
			DWORD b;
			WriteFile( fh, buffer, cur - buffer, &b, NULL );
		}

		LeaveCriticalSection( &cs );
	}

#define op_thread_transition	1
#define op_enter_func			2
#define op_leave_func			3

#pragma pack(push,1)
	struct OpRec
	{
		unsigned char opcode;
		UINT id;

		OpRec( unsigned char opcode, UINT id ) : opcode(opcode), id(id) {}
	};
#pragma pack(pop)

	void WriteThreadTransition( UINT managedThreadId )
	{
		OpRec o( op_thread_transition, managedThreadId );
		WriteData( (unsigned char*)&o, sizeof(o) );
	}

	void WriteEnterFunction( UINT functionId ) 
	{
		OpRec o( op_enter_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );
	}

	void WriteLeaveFunction( UINT functionId )
	{
		OpRec o( op_leave_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );
	}

	void WriteFunctionBinding( UINT functionId, std::wstring const & name ) {}
};

class Profiler * __inst;

class Profiler : public ProfilerBase
{
	std::map<UINT, UINT> seen_function;
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
		profiler->SetEnterLeaveFunctionHooks( Profiler::DispatchFunctionEnter, 0, 0 );
		return S_OK;
	}

	static void DispatchFunctionEnter( UINT functionId ) { __inst->OnFunctionEnter( functionId ); }
	static void DispatchFunctionLeave( UINT functionId ) { __inst->OnFunctionLeave( functionId ); }

	volatile UINT lastSeenThread;

	void ReportThreadTransition()
	{
		UINT currentManagedThread = *thread.get();
		if (currentManagedThread != InterlockedExchange( (LONG volatile *)&lastSeenThread, currentManagedThread ))
		{
			char sz[64];
			sprintf(sz, "switch to thread 0x%08x", currentManagedThread);
			Log(sz);

			writer.WriteThreadTransition( currentManagedThread );
		}
	}

	void OnFunctionEnter( UINT functionId )
	{
		ReportThreadTransition();

		if (!(seen_function[functionId]++))
		{
			char sz[1024];
			std::wstring ws = GetFunctionName( functionId );
			sprintf(sz, "0x%08x=%ws", functionId, ws.c_str());
			Log(sz);

			writer.WriteFunctionBinding( functionId, ws );
		}
		writer.WriteEnterFunction( functionId );
	}

	void OnFunctionLeave( UINT functionId )
	{
		ReportThreadTransition();
		writer.WriteLeaveFunction( functionId );
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

STDAPI DllGetClassObject( IID const & rclsid, IID const & riid, void ** ppv )
{
	static ClassFactory<Profiler> classFactory;

	return (CLSID_PROFILER == rclsid) ? 
		classFactory.QueryInterface( riid, ppv ) : E_OUTOFMEMORY;
}
