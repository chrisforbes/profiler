#pragma once

//requires <windows.h>

template <typename T>
class thread_ptr
{
	DWORD slot;

public:
	thread_ptr()
		: slot( TlsAlloc() )
	{
	}

	~thread_ptr()
	{
		TlsFree( slot );
	}

	T * get() { return (T*) TlsGetValue( slot ); }
	void set( T * value ) { TlsSetValue( slot, (void *)value ); }
};