#ifndef NATIVEHOST_DLL_H_
#define NATIVEHOST_DLL_H_

#include "hostfxr.h"
#include <windows.h>
#include <iostream>
#include <comutil.h>

// The max length of a function to be executed in a .NET class
constexpr auto max_function_name_size = 256;

// The max length of arguments to be parsed and passed to a .NET function
constexpr auto assembly_function_arguments_size = 12;

constexpr auto max_path = 260;

#define SHARED_API extern "C" __declspec(dllexport)

// Arguments for hosting the .NET Core runtime and loading an assembly
struct core_host_arguments
{
	const char_t	assembly_file_path[max_path];
	const char_t	core_root_path[max_path];
	unsigned char	verbose;
	const char_t	pipename[max_path];
};

// Arguments for executing a function located in a .NET assembly,
// with optional arguments passed to the function call
struct assembly_function_call
{
	const char_t assembly_path[max_function_name_size];
	const char_t type_name_qualified[max_function_name_size];
	const char_t method_name[max_function_name_size];
	const char_t delegate_type_name[max_function_name_size];

	const char_t payload[1024];
};

void WriteToPipe(const HANDLE pipeHandle, const std::string msg);

// ===============================================================================
// DLL exports used for starting, executing in, and stopping the .NET Core runtime

// Create a native function delegate for a function inside a .NET assembly
SHARED_API int CreateAssemblyDelegate(const assembly_function_call* arguments, void** pfnDelegate);

// Execute a function located in a .NET assembly by creating a native delegate
SHARED_API int ExecuteAssemblyFunction(const assembly_function_call* arguments);

// Host the .NET Core runtime in the current application
SHARED_API int StartCoreCLR(const core_host_arguments* arguments);

// Stop the .NET Core host in the current application
SHARED_API int UnloadRuntime();

#endif // NATIVEHOST_DLL_H_