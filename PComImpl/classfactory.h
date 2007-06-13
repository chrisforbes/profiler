#pragma once

template< typename T >
class ClassFactory : public IClassFactory
{
	long refcount;

public:
	ClassFactory()
		: refcount(1)
	{
	}

	// IUnknown implementation

	STDMETHOD_(ULONG, AddRef)() { return InterlockedIncrement( &refcount ); }
	STDMETHOD_(ULONG, Release)() { return InterlockedDecrement( &refcount ); }
	
	STDMETHOD(QueryInterface)( IID const & riid, void ** ppInterface )
	{
		if (riid == IID_IUnknown || riid == IID_IClassFactory)
			*ppInterface = this;
		else
		{
			*ppInterface = NULL;
			return E_NOINTERFACE;
		}

		AddRef();
		return S_OK;
	}

	// IClassFactory implementation

	STDMETHOD(LockServer)( BOOL ) { return S_OK; }

	STDMETHOD(CreateInstance)( IUnknown * pUnkOuter, IID const & riid, void ** ppInterface )
	{
		// aggregation is not supported.
		if (pUnkOuter)
			return CLASS_E_NOAGGREGATION;

		*ppInterface = new T;
		return S_OK;
	}
};

