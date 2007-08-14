#pragma once

template< typename T >
class ThreadPtr
{
	DWORD id;
public:
	ThreadPtr() : id( TlsAlloc() )
	{
	}

	T * get() const
	{
		return (T *)TlsGetValue( id );
	}

	void set( T const * x )
	{
		TlsSetValue( id, (void *) x );
	}

	virtual ~ThreadPtr()
	{
		TlsFree( id );
	}
};
