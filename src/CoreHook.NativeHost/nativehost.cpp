// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Headers for this file
#include "nativehost.h"

// Standard headers
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <iostream>
#include <vector>

// Provided by the AppHost NuGet package and installed as an SDK pack
#include "nethost.h"
#include "coreclr_delegates.h"
#include "hostfxr.h"

#ifdef WINDOWS
#include <Windows.h>

#define STR(s) L ## s
#define CH(c) L ## c
#define DIR_SEPARATOR L'\\'

#else
#include <dlfcn.h>
#include <limits.h>

#define STR(s) s
#define CH(c) c
#define DIR_SEPARATOR '/'
#define MAX_PATH PATH_MAX

#endif

using string_t = std::basic_string<char_t>;

typedef int (CORECLR_DELEGATE_CALLTYPE load_plugin_fn)(const void* ptr);

// Globals to hold hostfxr exports
hostfxr_initialize_for_runtime_config_fn init_fptr;
hostfxr_get_runtime_delegate_fn get_delegate_fptr;
hostfxr_close_fn close_fptr;

// Forward declarations
bool load_hostfxr();
load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* assembly);

/********************************************************************************************
 * Function used to load and activate .NET Core
 ********************************************************************************************/

 // Forward declarations
void* load_library(const char_t*);
void* get_export(void*, const char*);

#ifdef WINDOWS
void* load_library(const char_t* path)
{
	HMODULE h = ::LoadLibraryW(path);
	assert(h != nullptr);
	return (void*)h;
}
void* get_export(void* h, const char* name)
{
	void* f = ::GetProcAddress((HMODULE)h, name);
	assert(f != nullptr);
	return f;
}
#else
void* load_library(const char_t* path)
{
	void* h = dlopen(path, RTLD_LAZY | RTLD_LOCAL);
	assert(h != nullptr);
	return h;
}
void* get_export(void* h, const char* name)
{
	void* f = dlsym(h, name);
	assert(f != nullptr);
	return f;
}
#endif

// <SnippetLoadHostFxr>
// Using the nethost library, discover the location of hostfxr and get exports
bool load_hostfxr()
{
	// Pre-allocate a large buffer for the path to hostfxr
	char_t buffer[MAX_PATH];
	size_t buffer_size = sizeof(buffer) / sizeof(char_t);
	int rc = get_hostfxr_path(buffer, &buffer_size, nullptr);
	if (rc != 0) {
		return false;
	}

	// Load hostfxr and get desired exports
	void* lib = load_library(buffer);
	init_fptr = (hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
	get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
	close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

	return (init_fptr && get_delegate_fptr && close_fptr);
}
// </SnippetLoadHostFxr>

// <SnippetInitialize>
// Load and initialize .NET Core and get desired function pointer for scenario
load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const char_t* config_path)
{
	// Load .NET Core
	void* load_assembly_and_get_function_pointer = nullptr;
	hostfxr_handle cxt = nullptr;
	int rc = init_fptr(config_path, nullptr, &cxt);
	if (rc != 0 || cxt == nullptr)
	{
		std::cerr << "Init failed: " << std::hex << std::showbase << rc << std::endl;
		close_fptr(cxt);
		return nullptr;
	}

	// Get the load assembly function pointer
	rc = get_delegate_fptr(cxt, hdt_load_assembly_and_get_function_pointer, &load_assembly_and_get_function_pointer);

	if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
	{
		std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;
	}

	close_fptr(cxt);

	return (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
}

bool inline check_arg_length(const char_t* argument, size_t max_size)
{
	return (argument == nullptr || wcsnlen(argument, max_size) >= max_size) ? false : true;
}

static load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = nullptr;

// Host the .NET Core runtime in the current application
SHARED_API int StartCoreCLR(const core_host_arguments* arguments)
{
	if (arguments == nullptr
		|| !check_arg_length(arguments->assembly_file_path /* CoreHook.CoreLoader.dll */, MAX_PATH)
		|| !check_arg_length(arguments->core_root_path /* Current folder if standalone */, MAX_PATH))
	{
		return 1;// coreload::StatusCode::InvalidArgFailure;
	}

	// STEP 1: Load HostFxr and get exported hosting functions
	if (!load_hostfxr())
	{
		assert(false && "Failure: load_hostfxr()");
		return EXIT_FAILURE;
	}

	//while (!::IsDebuggerPresent())
	//	::Sleep(100); // to avoid 100% CPU load

	//DebugBreak();

	// STEP 2: Initialize and start the .NET Core runtime
	string_t host_path_str = arguments->assembly_file_path;
	const string_t config_path = host_path_str.substr(0, host_path_str.length() - 4) + STR(".runtimeconfig.json");
	load_assembly_and_get_function_pointer = get_dotnet_load_assembly(config_path.c_str());

	assert(load_assembly_and_get_function_pointer != nullptr && "Failure: StartCoreCLR()");

	return 0;
}


// Create a native function delegate for a function inside a .NET assembly
SHARED_API int CreateAssemblyDelegate(const assembly_function_call* arguments, void** pfnDelegate)
{
	if (arguments == nullptr
		|| !check_arg_length(arguments->assembly_path, max_function_name_size)
		|| !check_arg_length(arguments->type_name_qualified, max_function_name_size)
		|| !check_arg_length(arguments->method_name, max_function_name_size)
		|| !check_arg_length(arguments->delegate_type_name, max_function_name_size))
	{
		return 1;// coreload::StatusCode::InvalidArgFailure;
	}

	int rc = load_assembly_and_get_function_pointer(
		arguments->assembly_path,
		arguments->type_name_qualified,
		arguments->method_name,
		UNMANAGEDCALLERSONLY_METHOD,//	wcsnlen(arguments->delegate_type_name, 1) > 0 ? arguments->delegate_type_name : nullptr,
		nullptr /* reserved */,
		pfnDelegate);

	assert(rc == 0 && pfnDelegate != nullptr && "Failure: load_assembly_and_get_function_pointer()");

	return rc;
}

// Execute a function located in a .NET assembly by creating a native delegate
SHARED_API int ExecuteAssemblyFunction(const assembly_function_call* arguments)
{
	load_plugin_fn* execute_delegate = nullptr;
	auto exit_code = CreateAssemblyDelegate(arguments, reinterpret_cast<PVOID*>(&execute_delegate));

	if (SUCCEEDED(exit_code))
	{
		exit_code = execute_delegate(BSTR(arguments->payload));
	}

	return exit_code;
}

//
//bool clrstring(const string_t& str, std::vector<char>* out)
//{
//	out->clear();
//
//	// Pass -1 as we want explicit null termination in the char buffer.
//	size_t size = ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), -1, nullptr, 0, nullptr, nullptr);
//	if (size == 0)
//	{
//		return false;
//	}
//	out->resize(size, '\0');
//	return ::WideCharToMultiByte(CP_UTF8, 0, str.c_str(), -1, out->data(), out->size(), nullptr, nullptr) != 0;
//}

// Shutdown the .NET Core runtime
SHARED_API int UnloadRuntime()
{
	// Not implemented
	return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
	default:
		break;
	}
	return TRUE;
}