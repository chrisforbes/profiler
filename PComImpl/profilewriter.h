#pragma once

// 2MB buffer!
#define BUFFER_SIZE		2048 * 1024

#define op_thread_transition	1
#define op_enter_func			2
#define op_leave_func			3
#define op_setfreq				4
#define op_tail_func			5

class ProfileWriter
{
	HANDLE fh;
	unsigned char buffer[ BUFFER_SIZE ];
	unsigned char * cur;
	CRITICAL_SECTION cs;

public:
	ProfileWriter( std::string const & filename )
		: lastThreadId(0)
	{
		fh = CreateFileA( filename.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, 0 );
		cur = buffer;
		InitializeCriticalSection( &cs );
	}

	~ProfileWriter()
	{
		Flush();
		CloseHandle( fh );
		delete[] buffer;
		DeleteCriticalSection( &cs );
	}

#pragma pack(push,1)
	struct OpRec
	{
		unsigned char opcode;
		UINT id;
		__int64 timestamp;

		OpRec( unsigned char opcode, UINT id ) : opcode(opcode), id(id), timestamp(0)
		{
			__int64 t;

			if (TRUE != QueryPerformanceCounter( ( LARGE_INTEGER * ) &t ))
				timestamp = GetLastError();

			timestamp = t;
		}
	};
#pragma pack(pop)

	UINT lastThreadId;

	void WriteClockFrequency()
	{
		OpRec o( op_setfreq, 0 );

		__int64 t;
		QueryPerformanceFrequency( (LARGE_INTEGER *) &t );
		o.timestamp = t;

		WriteData( (unsigned char*) &o, sizeof(o) );
	}

	void WriteEnterFunction( UINT functionId, ICorProfilerInfo2 * prof ) 
	{
		EnterCriticalSection( &cs );

		UINT threadId;
		prof->GetCurrentThreadID( &threadId );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_enter_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

	void WriteLeaveFunction( UINT functionId, ICorProfilerInfo2 * prof )
	{
		EnterCriticalSection( &cs );

		UINT threadId;
		prof->GetCurrentThreadID( &threadId );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_leave_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

	void WriteTailFunction( UINT functionId, ICorProfilerInfo2 * prof )
	{
		EnterCriticalSection( &cs );

		UINT threadId;
		prof->GetCurrentThreadID( &threadId );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_tail_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

private:
	void WriteThreadTransition( UINT managedThreadId )
	{
		OpRec o( op_thread_transition, managedThreadId );
		WriteData( (unsigned char*)&o, sizeof(o) );
	}

	void WriteData( unsigned char * data, unsigned int length )
	{
		DWORD b;
		if (cur + length >= buffer + BUFFER_SIZE)
		{
			WriteFile( fh, buffer, (DWORD)(cur - buffer), &b, NULL );
			cur = buffer;
		}

		memcpy( cur, data, length );
		cur += length;
	}

	void Flush()
	{
		EnterCriticalSection( &cs );

		if (cur != buffer)
		{
			DWORD b;
			WriteFile( fh, buffer, (DWORD)(cur - buffer), &b, NULL );
		}

		LeaveCriticalSection( &cs );
	}
};

