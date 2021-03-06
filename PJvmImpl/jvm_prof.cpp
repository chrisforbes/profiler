
// IJW Profiler - Backend agent for the Java VM
// C. Forbes

// This library requires J2SE 1.5.0 (Java 5) or later to be installed on the development machine.
// Tested with J2SE 1.6.0 Update 1 JDK

// You'll need to have these on your search path:
//	$jdk/include/
//	$jdk/include/win32/
//	$jdk/lib/

#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>
#include <jvmti.h>
#include <memory>
#include <string>
#include <map>
#include <stack>

#include "../pcomimpl/thread_ptr.h"
#include "../pcomimpl/function_stack.h"
#include "../pcomimpl/profilewriter.h"

char const * txtProfileEnv = "ijwprof_txt";
char const * binProfileEnv = "ijwprof_bin";

std::string GetEnv( std::string const & name )
{
	static char buf[MAX_PATH];
	GetEnvironmentVariableA( name.c_str(), buf, MAX_PATH );
	return buf;
}

jvmtiEnv * jvmti = NULL;
FILE * logfile = NULL;
std::map< jmethodID, bool > interesting;
ProfileWriter * writer = NULL;
FunctionStack fns;

bool IsSystemMethod( std::string const & s )
{
	if (s.find("java.") == 0)
		return true;
	if (s.find("javax.") == 0)
		return true;
	if (s.find("sun.") == 0)
		return true;

	return false;
}

void HackJavaClassName( char * s )
{
	while (*s)
	{
		if (*s == '/')
			*s = '.';
		if (*s == ';')
			*s = 0;
		s++;
	}
}

void ExportMethodName( jvmtiEnv * ti_env, jmethodID method )
{
	if (interesting.find(method) != interesting.end())
		return;

	char *method_name = 0, *clazz_name = 0;
	jclass clazz;

	ti_env->GetMethodName( method, &method_name, NULL, NULL );
	ti_env->GetMethodDeclaringClass( method, &clazz );

	ti_env->GetClassSignature( clazz, &clazz_name, NULL );
	HackJavaClassName( clazz_name );

	fprintf( logfile, "0x%08x=%s::%s\n", (size_t)method, clazz_name + 1, method_name );

	interesting[ method ] = !IsSystemMethod( clazz_name + 1);

	ti_env->Deallocate( (unsigned char *) clazz_name );
	ti_env->Deallocate( (unsigned char *) method_name );
}

void JNICALL MethodEntry( jvmtiEnv * ti_env, JNIEnv * env, jthread thread, jmethodID method )
{
	ExportMethodName( ti_env, method );

	bool parentInteresting = fns.Empty() || fns.Peek();
	bool functionInteresting = interesting[ method ];

	fns.Push( functionInteresting );

	if (parentInteresting || functionInteresting)
		writer->WriteEnterFunction( (size_t)method, (size_t)thread );
}

void JNICALL MethodExit( jvmtiEnv * ti_env, JNIEnv * env, jthread thread, jmethodID method,
						jboolean was_popped_by_exception, jvalue return_value )
{
	bool functionInteresting = fns.Pop();
	bool parentInteresting = fns.Empty() || fns.Peek();

	if (parentInteresting || functionInteresting)
		writer->WriteLeaveFunction( (size_t)method, (size_t)thread );
}

JNIEXPORT jint JNICALL Agent_OnLoad( JavaVM * jvm, char * options, void * reserved )
{
	logfile = fopen( GetEnv( txtProfileEnv ).c_str(), "w" );
	writer = new ProfileWriter( GetEnv( binProfileEnv ) );

	jvm->GetEnv( (void **) &jvmti, JVMTI_VERSION_1_0 );

	// set caps

	jvmtiCapabilities caps;
	jvmti->GetCapabilities( &caps );

	caps.can_generate_method_entry_events = 1;
	caps.can_generate_method_exit_events = 1;

	jvmti->AddCapabilities( &caps );

	// bind callbacks

	jvmtiEventCallbacks callbacks;
	memset( &callbacks, 0, sizeof( callbacks ) );

	callbacks.MethodEntry = MethodEntry;
	callbacks.MethodExit = MethodExit;

	jvmti->SetEventCallbacks( &callbacks, sizeof(jvmtiEventCallbacks) );
	
	jvmti->SetEventNotificationMode( JVMTI_ENABLE, JVMTI_EVENT_METHOD_ENTRY, NULL );
	jvmti->SetEventNotificationMode( JVMTI_ENABLE, JVMTI_EVENT_METHOD_EXIT, NULL );

	writer->WriteClockFrequency();
	writer->WriteTimeBase();

	return JNI_OK;
}

JNIEXPORT void JNICALL Agent_OnUnload( JavaVM * jvm )
{
	jvmti->SetEventCallbacks( NULL, sizeof( jvmtiEventCallbacks ) );
	writer->Flush();
	delete writer;
}