#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <stdio.h>

#include <cor.h>
#include <corprof.h>

#define ARRAY_SIZE( s ) (sizeof( s ) / sizeof( s[0] ))
#define MAX_LENGTH 256

HINSTANCE hinst;
static const char * progId = "IjwProfiler";
#define PROFILER_GUID "{C1E9FE1F-F517-45c0-BB0E-EFAECC9401FC}"

extern const GUID __declspec( selectany ) CLSID_PROFILER;

BOOL WINAPI DllMain( HINSTANCE hInstance, DWORD dwReason, void * )
{
	if (DLL_PROCESS_ATTACH == dwReason)
		DisableThreadLibraryCalls( hinst = hInstance );

	return TRUE;
}

BOOL DeleteKey( const char *szKey,
                 const char *szSubkey )
{
    char rcKey[MAX_LENGTH]; // buffer for the full key name.


    // init the key with the base key name.
    strcpy( rcKey, szKey );

    // append the subkey name (if there is one).
    if ( szSubkey != NULL )
    {
        strcat( rcKey, "\\" );
        strcat( rcKey, szSubkey );
    }

    // delete the registration key.
    RegDeleteKeyA( HKEY_CLASSES_ROOT, rcKey );
    
    return TRUE;
}

BOOL SetKeyAndValue( const char *szKey,
                              const char *szSubkey,
                              const char *szValue )
{
    HKEY hKey;              // handle to the new reg key.
    char rcKey[MAX_LENGTH]; // buffer for the full key name.

    // init the key with the base key name.
    strcpy( rcKey, szKey );

    // append the subkey name (if there is one).
    if ( szSubkey != NULL )
    {
        strcat( rcKey, "\\" );
        strcat( rcKey, szSubkey );
    }

    // create the registration key.
    if ( RegCreateKeyExA( HKEY_CLASSES_ROOT, 
                          rcKey, 
                          0, 
                          NULL,
                          REG_OPTION_NON_VOLATILE, 
                          KEY_ALL_ACCESS, 
                          NULL,
                          &hKey, 
                          NULL ) == ERROR_SUCCESS )
    {
        // set the value (if there is one).
        if ( szValue != NULL )
        {
            RegSetValueExA( hKey, NULL, 0, REG_SZ, (BYTE *)szValue,
                            (DWORD)(((strlen(szValue) + 1) * sizeof (char))));
        }
        
        RegCloseKey( hKey );

        return TRUE;
    }

    return FALSE;   
}

BOOL SetRegValue(char *szKeyName, char *szKeyword, char *szValue)
{
    HKEY hKey; // handle to the new reg key.

    // create the registration key.
    if ( RegCreateKeyExA( HKEY_CLASSES_ROOT, 
                          szKeyName, 0,  NULL,
                          REG_OPTION_NON_VOLATILE, 
                          KEY_ALL_ACCESS, 
                          NULL, &hKey, 
                          NULL) == ERROR_SUCCESS )
    {
        // set the value (if there is one).
        if ( szValue != NULL )
        {
            RegSetValueExA( hKey, szKeyword, 0, REG_SZ, 
                            (BYTE *)szValue, 
                            (DWORD)((strlen(szValue) + 1) * sizeof ( char )));
        }

        RegCloseKey( hKey );
        
        return TRUE;
    }
    
    return FALSE;
}

HRESULT RegisterClassBase( IID const & rclsid,
                            const char *szDesc,                 
                            const char *szProgID,               
                            const char *szIndepProgID,          
                            char *szOutCLSID )              
{
    char szID[64];     // the class ID to register.
    OLECHAR szWID[64]; // helper for the class ID to register.

    StringFromGUID2( rclsid, szWID, ARRAY_SIZE( szWID ) );
    WideCharToMultiByte(CP_ACP, 0, szWID, -1, szID, sizeof(szID), NULL, NULL);

    strcpy( szOutCLSID, "CLSID\\" );
    strcat( szOutCLSID, szID );

    // create ProgID keys.
    SetKeyAndValue( szProgID, NULL, szDesc );
    SetKeyAndValue( szProgID, "CLSID", szID );

    // create VersionIndependentProgID keys.
    SetKeyAndValue( szIndepProgID, NULL, szDesc );
    SetKeyAndValue( szIndepProgID, "CurVer", szProgID );
    SetKeyAndValue( szIndepProgID, "CLSID", szID );

    // create entries under CLSID.
    SetKeyAndValue( szOutCLSID, NULL, szDesc );
    SetKeyAndValue( szOutCLSID, "ProgID", szProgID );
    SetKeyAndValue( szOutCLSID, "VersionIndependentProgID", szIndepProgID );
    SetKeyAndValue( szOutCLSID, "NotInsertable", NULL );
    
    return S_OK;
}

HRESULT UnregisterClassBase( IID const & rclsid,
                            const char *szProgID,
                            const char *szIndepProgID,
                            char *szOutCLSID )
{
    char szID[64];     // the class ID to register.
    OLECHAR szWID[64]; // helper for the class ID to register.

    StringFromGUID2( rclsid, szWID, ARRAY_SIZE( szWID ) );
    WideCharToMultiByte(CP_ACP, 0, szWID, -1, szID, sizeof(szID), NULL, NULL);

    strcpy( szOutCLSID, "CLSID\\" );
    strcat( szOutCLSID, szID );

    // delete the version independant prog ID settings.
    DeleteKey( szIndepProgID, "CurVer" );
    DeleteKey( szIndepProgID, "CLSID" );
    RegDeleteKeyA( HKEY_CLASSES_ROOT, szIndepProgID );

    // delete the prog ID settings.
    DeleteKey( szProgID, "CLSID" );
    RegDeleteKeyA( HKEY_CLASSES_ROOT, szProgID );

    // delete the class ID settings.
    DeleteKey( szOutCLSID, "ProgID" );
    DeleteKey( szOutCLSID, "VersionIndependentProgID" );
    DeleteKey( szOutCLSID, "NotInsertable" );
    RegDeleteKeyA( HKEY_CLASSES_ROOT, szOutCLSID );
    
    return S_OK;
}

STDAPI DllUnregisterServer()
{
    char szID[64];         // the class ID to unregister.
    char szCLSID[64];      // CLSID\\szID.
    OLECHAR szWID[64];     // helper for the class ID to unregister.
    char rcProgID[128];    // szProgIDPrefix.szClassProgID
    char rcIndProgID[128]; // rcProgID.iVersion


    // format the prog ID values.
	sprintf( rcProgID, "%s.%s", progId, PROFILER_GUID );
    sprintf( rcIndProgID, "%s.%d", rcProgID, 1 );

    UnregisterClassBase( CLSID_PROFILER, rcProgID, rcIndProgID, szCLSID );
    DeleteKey( szCLSID, "InprocServer32" );

    StringFromGUID2(CLSID_PROFILER, szWID, ARRAY_SIZE( szWID ) );
    WideCharToMultiByte(CP_ACP, 0, szWID, -1, szID, sizeof(szID), NULL,NULL);

    DeleteKey( "CLSID", szCLSID );

    return S_OK; 
}

STDAPI DllRegisterServer()
{
	char szModule[_MAX_PATH];

	DllUnregisterServer();
	GetModuleFileNameA( hinst, szModule, _MAX_PATH );

	char rcCLSID[MAX_LENGTH];
	char rcProgID[MAX_LENGTH];
	char rcIndProgID[MAX_LENGTH];
	char rcInproc[MAX_LENGTH + 2];

	sprintf( rcIndProgID, "%s.%s", progId, PROFILER_GUID );
	sprintf( rcProgID, "%s.%d", rcIndProgID, 1 );

	HRESULT hr = RegisterClassBase( CLSID_PROFILER, "Listener", rcProgID, rcIndProgID, rcCLSID );

	if ( SUCCEEDED(hr))
	{
		SetKeyAndValue( rcCLSID, "InprocServer32", szModule );
		sprintf( rcInproc, "%s\\%s", rcCLSID, "InprocServer32" );
		SetRegValue( rcInproc, "ThreadingModel", "Both" );
	}
	else
		DllUnregisterServer();
	
	return hr;
}
