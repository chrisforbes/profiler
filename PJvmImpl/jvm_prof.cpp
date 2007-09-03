
// IJW Profiler - Backend agent for the Java VM
// C. Forbes

// This library requires J2SE 1.5.0 (Java 5) or later to be installed on the development machine.
// Tested with J2SE 1.6.0 Update 1 JDK

// You'll need to have these on your search path:
//	$jdk/include/
//	$jdk/include/win32/
//	$jdk/lib/

#include <jvmti.h>
#include <memory>

jvmtiEnv * jvmti = NULL;

void JNICALL MethodEntry( jvmtiEnv * ti_env, JNIEnv * env, jthread thread, jmethodID method )
{
	// todo: log method entry
	// todo: bind method name
}

void JNICALL MethodExit( jvmtiEnv * ti_env, JNIEnv * env, jthread thread, jmethodID method,
						jboolean was_popped_by_exception, jvalue return_value )
{
	// todo: log method exit
}

JNIEXPORT jint JNICALL Agent_OnLoad( JavaVM * jvm, char * options, void * reserved )
{
	jvm->GetEnv( (void **) &jvmti, JVMTI_VERSION_1_0 );

	jvmtiEventCallbacks callbacks;
	memset( &callbacks, 0, sizeof( callbacks ) );

	callbacks.MethodEntry = MethodEntry;
	callbacks.MethodExit = MethodExit;

	jvmti->SetEventCallbacks( &callbacks, sizeof(jvmtiEventCallbacks) );
	
	jvmti->SetEventNotificationMode( JVMTI_ENABLE, JVMTI_EVENT_METHOD_ENTRY, NULL );
	jvmti->SetEventNotificationMode( JVMTI_ENABLE, JVMTI_EVENT_METHOD_EXIT, NULL );

	return JNI_OK;
}

JNIEXPORT void JNICALL Agent_OnUnload( JavaVM * jvm )
{
	// todo: flush data to disk
	jvmti->SetEventCallbacks( NULL, sizeof( jvmtiEventCallbacks ) );
}