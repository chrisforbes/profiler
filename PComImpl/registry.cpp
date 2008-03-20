#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>

#include <cor.h>
#include <corprof.h>

#include <string>
#include <shlwapi.h>

HINSTANCE hinst;
static const char * progId = "IjwProfiler";
#define PROFILER_GUID "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}"

extern const GUID __declspec( selectany ) CLSID_PROFILER;

#pragma comment( lib, "shlwapi.lib" )

BOOL WINAPI DllMain( HINSTANCE hInstance, DWORD dwReason, void * )
{
	if (DLL_PROCESS_ATTACH == dwReason)
		DisableThreadLibraryCalls( hinst = hInstance );

	return TRUE;
}

HKEY OpenKey( HKEY parent, std::string const & name )
{
	HKEY result;
	if (ERROR_SUCCESS != RegCreateKeyExA( parent, name.c_str(), 0, 0, 
		REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, 0, &result, 0 ))
	{
		::MessageBoxA( 0, name.c_str(), "Failed to create key", MB_OK );
	}
	return result;
}

void CloseKey( HKEY key )
{
	RegCloseKey( key );
}

void SetValue( HKEY key, std::string const & name, std::string const & value )
{
	RegSetValueExA( key, name.c_str(), 0, REG_SZ, (BYTE const *) value.c_str(), value.size() );
}

STDAPI DllUnregisterServer()
{
	HKEY kSoftware = OpenKey( HKEY_CURRENT_USER, "Software" );
	HKEY kClasses = OpenKey( kSoftware, "Classes" );
	HKEY kClsid = OpenKey( kClasses, "CLSID" );
	HKEY kApp = OpenKey( kClsid, PROFILER_GUID );

	SHDeleteKeyA( kClsid, PROFILER_GUID );
	
	CloseKey( kClsid );
	CloseKey( kClasses );
	CloseKey( kSoftware );

	return S_OK;
}

STDAPI DllRegisterServer()
{
	char szModule[_MAX_PATH];

	GetModuleFileNameA( hinst, szModule, _MAX_PATH );

	HKEY kSoftware = OpenKey( HKEY_CURRENT_USER, "Software" );
	HKEY kClasses = OpenKey( kSoftware, "Classes" );
	HKEY kClsid = OpenKey( kClasses, "CLSID" );
	HKEY kGuid = OpenKey( kClsid, PROFILER_GUID );
	HKEY kInProc = OpenKey( kGuid, "InProcServer32" );

	SetValue( kGuid, "", "IJW Sampling Profiler" );
	SetValue( kInProc, "", szModule );
	SetValue( kInProc, "ThreadingModel", "Both" );

	CloseKey( kInProc );
	CloseKey( kGuid );
	CloseKey( kClsid );
	CloseKey( kClasses );
	CloseKey( kSoftware );
	
	return S_OK;
}
