#pragma once

#include <process.h>

// 2MB buffer!
#define BUFFER_SIZE		2048 * 1024

#define IOCK_IO		0
#define IOCK_TERM	1

struct io_params
{
	HANDLE iocp;
	HANDLE f;
};

struct io_packet
{
	int action;
	unsigned char * buffer;
	int length;
};

void DoIo( void * p )
{
	io_params * params = (io_params *) p;
	DWORD bytes;
	io_packet * packet;
	OVERLAPPED * overlapped;
	while( ::GetQueuedCompletionStatus( params->iocp, &bytes, (PULONG_PTR) &packet, &overlapped, INFINITE ) )
	{
		if (packet->action == IOCK_TERM)
		{
			::CloseHandle( params->iocp );
			::CloseHandle( params->f );
			delete packet;
			delete params;
			return;
		}

		if (packet->action == IOCK_IO)
		{
			::WriteFile( params->f, packet->buffer, packet->length, &bytes, 0 );
			delete [] packet->buffer;
			delete packet;
		}

		// shouldnt get here!
	}
}

#define op_thread_transition	1
#define op_enter_func			2
#define op_leave_func			3
#define op_setfreq				4
#define op_tail_func			5
#define op_timebase				6

class ProfileWriter
{
	HANDLE iocp;
	unsigned char * buffer;
	unsigned char * cur;
	CRITICAL_SECTION cs;

public:
	ProfileWriter( std::string const & filename )
		: lastThreadId(0)
	{
		cur = buffer = new unsigned char[ BUFFER_SIZE ];
		InitializeCriticalSection( &cs );

		io_params * params = new io_params;
		params->f = ::CreateFileA( filename.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, 0 );
		params->iocp = iocp = ::CreateIoCompletionPort( INVALID_HANDLE_VALUE, 0, 0, 2 );
		_beginthread( DoIo, 0, (void *)params );
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
		EnterCriticalSection( &cs );
		OpRec o( op_setfreq, 0 );

		__int64 t;
		QueryPerformanceFrequency( (LARGE_INTEGER *) &t );
		o.timestamp = t;

		WriteData( (unsigned char*) &o, sizeof(o) );
		LeaveCriticalSection( &cs );
	}

	void WriteEnterFunction( UINT functionId, UINT threadId ) 
	{
		EnterCriticalSection( &cs );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_enter_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

	void WriteLeaveFunction( UINT functionId, UINT threadId )
	{
		EnterCriticalSection( &cs );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_leave_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

	void WriteTailFunction( UINT functionId, UINT threadId )
	{
		EnterCriticalSection( &cs );

		if (threadId != lastThreadId)
			WriteThreadTransition( lastThreadId = threadId );

		OpRec o( op_tail_func, functionId );
		WriteData( (unsigned char*)&o, sizeof(o) );

		LeaveCriticalSection( &cs );
	}

	void WriteTimeBase()
	{
		EnterCriticalSection( &cs );
		OpRec o( op_timebase, 0 );
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
		if (cur + length >= buffer + BUFFER_SIZE)
			__WriteData();

		memcpy( cur, data, length );
		cur += length;
	}

	void __WriteData()
	{
		io_packet * packet = new io_packet;
		packet->action = IOCK_IO;
		packet->buffer = buffer;
		packet->length = cur - buffer;
		::PostQueuedCompletionStatus( iocp, 1 /* dummy */, (ULONG_PTR) packet, 0 );
		buffer = new unsigned char[ BUFFER_SIZE ];
		cur = buffer;
	}

public:
	void Flush()
	{
		EnterCriticalSection( &cs );

		if (cur != buffer)
			__WriteData();

		io_packet * packet = new io_packet;
		packet->action = IOCK_TERM;
		::PostQueuedCompletionStatus( iocp, 1 /* dummy */, (ULONG_PTR) packet, 0 );

		LeaveCriticalSection( &cs );

		DeleteCriticalSection( &cs );
	}
};

