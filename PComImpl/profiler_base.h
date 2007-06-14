#pragma once

class ProfilerBase : public ICorProfilerCallback2
{
protected:
	ICorProfilerInfo2 * profiler;
	DWORD eventMask;
	FILE * f;

public:
	ProfilerBase( DWORD eventMask )
		: eventMask( eventMask )
	{
		f = fopen( "c:\\profile.txt", "w" );
	}

	ProfilerBase()
		: eventMask( COR_PRF_ALL )
	{
	}

	void Log( char const * s )
	{
		fprintf(f, "%s\n", s );
		fflush(f);
	}

	STDMETHOD_(ULONG,AddRef)() { return S_OK; }
	STDMETHOD_(ULONG,Release)() { return S_OK; }
	STDMETHOD(QueryInterface)( IID const & riid, void ** ppInterface ) 
	{ 
		if (riid == IID_IUnknown || riid == IID_ICorProfilerCallback || riid == IID_ICorProfilerCallback2)
		{
			*ppInterface = (void*)this;
			return S_OK;
		}
		Log("Query Interface failed");
		return E_NOINTERFACE;
	}

	STDMETHOD(Initialize)( IUnknown * pCorProfilerInfoUnk ) 
	{
		HRESULT hr = pCorProfilerInfoUnk->QueryInterface( IID_ICorProfilerInfo2, (void **)&profiler );
		if (FAILED(hr))
		{
			Log("initialize failed");
			return E_INVALIDARG;
		}

		profiler->SetEventMask( eventMask );
		return S_OK;
	}

	STDMETHOD(Shutdown)()
	{
		return S_OK;
	}

	STDMETHOD(AppDomainCreationStarted)(UINT appDomainId) { return E_NOTIMPL; }
	STDMETHOD(AppDomainCreationFinished)(UINT appDomainId, HRESULT hrStatus ) { return E_NOTIMPL; }
	STDMETHOD(AppDomainShutdownStarted)(UINT appDomainId) { return E_NOTIMPL; }
	STDMETHOD(AppDomainShutdownFinished)(UINT appDomainId, HRESULT hrStatus ) { return E_NOTIMPL; }

	STDMETHOD(AssemblyLoadStarted)(UINT assemblyId) { return E_NOTIMPL; }
	STDMETHOD(AssemblyLoadFinished)(UINT assemblyId, HRESULT hrStatus) { return E_NOTIMPL; }
	STDMETHOD(AssemblyUnloadStarted)(UINT assemblyId) { return E_NOTIMPL; }
	STDMETHOD(AssemblyUnloadFinished)(UINT assemblyId, HRESULT hrStatus) { return E_NOTIMPL; }

	STDMETHOD(ModuleLoadStarted)(UINT moduleId) { return E_NOTIMPL; }
	STDMETHOD(ModuleLoadFinished)(UINT moduleId, HRESULT hrStatus) { return E_NOTIMPL; }
	STDMETHOD(ModuleUnloadStarted)(UINT moduleId) { return E_NOTIMPL; }
	STDMETHOD(ModuleUnloadFinished)(UINT moduleId, HRESULT hrStatus) { return E_NOTIMPL; }

	STDMETHOD(ModuleAttachedToAssembly)(UINT moduleId, UINT assemblyId) { return E_NOTIMPL; }

	STDMETHOD(ClassLoadStarted)(UINT classId) { return E_NOTIMPL; }
	STDMETHOD(ClassLoadFinished)(UINT classId, HRESULT hrStatus) { return E_NOTIMPL; }
	STDMETHOD(ClassUnloadStarted)(UINT classId) { return E_NOTIMPL; }
	STDMETHOD(ClassUnloadFinished)(UINT classId, HRESULT hrStatus) { return E_NOTIMPL; }

	STDMETHOD(FunctionUnloadStarted)(UINT functionId) { return E_NOTIMPL; }

	STDMETHOD(JITCompilationStarted)(UINT functionId, BOOL fIsSafeToBlock) { return E_NOTIMPL; }
	STDMETHOD(JITCompilationFinished)(UINT functionId, HRESULT hrStatus, BOOL fIsSafeToBlock) { return E_NOTIMPL; }

	STDMETHOD(JITCachedFunctionSearchStarted)(UINT functionId, BOOL * pbUseCachedFunction ) { return E_NOTIMPL; }
	STDMETHOD(JITCachedFunctionSearchFinished)(UINT functionId, COR_PRF_JIT_CACHE result ) { return E_NOTIMPL; }
	STDMETHOD(JITFunctionPitched)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(JITInlining)(UINT callerId, UINT calleeId, BOOL * pfShouldInline) { return E_NOTIMPL; }

	STDMETHOD(ThreadCreated)(UINT threadId) { return E_NOTIMPL; }
	STDMETHOD(ThreadDestroyed)(UINT threadId) { return E_NOTIMPL; }
	STDMETHOD(ThreadAssignedToOSThread)(UINT managed, DWORD os) { return E_NOTIMPL; }

	STDMETHOD(RemotingClientInvocationStarted)() { return E_NOTIMPL; }
	STDMETHOD(RemotingClientInvocationFinished)() { return E_NOTIMPL; }
	STDMETHOD(RemotingClientSendingMessage)( GUID * pCookie, BOOL fIsAsync ) { return E_NOTIMPL; }
	STDMETHOD(RemotingClientReceivingReply)( GUID * pCookie, BOOL fIsAsync ) { return E_NOTIMPL; }

	STDMETHOD(RemotingServerInvocationStarted)() { return E_NOTIMPL; }
	STDMETHOD(RemotingServerInvocationReturned)() { return E_NOTIMPL; }
	STDMETHOD(RemotingServerReceivingMessage)( GUID * pCookie, BOOL fIsAsync ) { return E_NOTIMPL; }
	STDMETHOD(RemotingServerSendingReply)( GUID * pCookie, BOOL fIsAsync ) { return E_NOTIMPL; }

	STDMETHOD(UnmanagedToManagedTransition)(UINT functionId, COR_PRF_TRANSITION_REASON reason ) { return E_NOTIMPL; }
	STDMETHOD(ManagedToUnmanagedTransition)(UINT functionId, COR_PRF_TRANSITION_REASON reason ) { return E_NOTIMPL; }

	STDMETHOD(RuntimeSuspendStarted)( COR_PRF_SUSPEND_REASON reason ) { return E_NOTIMPL; }
	STDMETHOD(RuntimeSuspendFinished)() { return E_NOTIMPL; }
	STDMETHOD(RuntimeSuspendAborted)() { return E_NOTIMPL; }

	STDMETHOD(RuntimeResumeStarted)() { return E_NOTIMPL; }
	STDMETHOD(RuntimeResumeFinished)() { return E_NOTIMPL; }

	STDMETHOD(RuntimeThreadSuspended)( UINT threadId ) { return E_NOTIMPL; }
	STDMETHOD(RuntimeThreadResumed)( UINT threadId ) { return E_NOTIMPL; }

	STDMETHOD(MovedReferences)( ULONG count, UINT oldIdRange[], UINT newIdRange[], ULONG idRangeLength[] ) { return E_NOTIMPL; }
	STDMETHOD(ObjectAllocated)(UINT objectId, UINT classId) { return E_NOTIMPL; }
	STDMETHOD(ObjectsAllocatedByClass)(ULONG classes, UINT classIds[], ULONG objectCounts[]) { return E_NOTIMPL; }
	STDMETHOD(ObjectReferences)(UINT objectId, UINT classId, ULONG numRefs, UINT refIds[] ) { return E_NOTIMPL; }
	STDMETHOD(RootReferences)(ULONG numRootRefs, UINT refs[] ) { return E_NOTIMPL; }
	STDMETHOD(ExceptionThrown)(UINT objectId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionSearchFunctionEnter)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionSearchFunctionLeave)() { return E_NOTIMPL; }
	STDMETHOD(ExceptionSearchFilterEnter)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionSearchFilterLeave)() { return E_NOTIMPL; }
	STDMETHOD(ExceptionSearchCatcherFound)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionOSHandlerEnter)(UINT_PTR) { return E_NOTIMPL; }
	STDMETHOD(ExceptionOSHandlerLeave)(UINT_PTR) { return E_NOTIMPL; }
	STDMETHOD(ExceptionUnwindFunctionEnter)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionUnwindFunctionLeave)() { return E_NOTIMPL; }
	STDMETHOD(ExceptionUnwindFinallyEnter)(UINT functionId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionUnwindFinallyLeave)() { return E_NOTIMPL; }
	STDMETHOD(ExceptionCatcherEnter)(UINT functionId, UINT objectId) { return E_NOTIMPL; }
	STDMETHOD(ExceptionCatcherLeave)() { return E_NOTIMPL; }

	STDMETHOD(COMClassicVTableCreated)( UINT classId, GUID const & iid, void * vt, ULONG slots ) { return E_NOTIMPL; }
	STDMETHOD(COMClassicVTableDestroyed)( UINT classId, GUID const & iid, void * vt ) { return E_NOTIMPL; }

	STDMETHOD(ExceptionCLRCatcherFound)() { return E_NOTIMPL; }
	STDMETHOD(ExceptionCLRCatcherExecute)() { return E_NOTIMPL; }

	STDMETHOD(ThreadNameChanged)(UINT threadId, ULONG cchName, WCHAR name[]) { return E_NOTIMPL; }
	STDMETHOD(GarbageCollectionStarted)(int gen, BOOL gens[], COR_PRF_GC_REASON reason) { return E_NOTIMPL; }
	STDMETHOD(SurvivingReferences)(ULONG, UINT o[], ULONG l[]) { return E_NOTIMPL; }
	STDMETHOD(GarbageCollectionFinished)() { return E_NOTIMPL; }
	STDMETHOD(FinalizeableObjectQueued)(DWORD finalizerFlags, UINT object) { return E_NOTIMPL; }
	STDMETHOD(RootReferences2)(ULONG, UINT ids[], COR_PRF_GC_ROOT_KIND rootkinds[], 
		COR_PRF_GC_ROOT_FLAGS flags[], UINT_PTR rootids[] ) { return E_NOTIMPL; }

	STDMETHOD(HandleCreated)(UINT h, UINT obj) { return E_NOTIMPL; }
	STDMETHOD(HandleDestroyed)(UINT h) { return E_NOTIMPL; }
};

