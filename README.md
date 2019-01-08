# CoreHook

[![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/CoreHook.svg?style=flat-square&colorB=f97356)](https://www.nuget.org/packages/CoreHook)

A library that simplifies intercepting application function calls using managed code and the .NET Core runtime. 

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook). 

## Contents

- [Build Status](#build-status)
- [Features](#features)
- [Supported Platforms](#supported-platforms)
- [Tested Platforms](#tested-platforms)
- [Dependencies](#dependencies)
- [Examples](#examples)
- [Plugin Examples](#plugin-examples)
- [Usage](#usage)
    - [Windows](#windows)
- [Contributing](#contributing)
- [License](#license)
- [Credits](#credits)

## Build status

| Build server    | Platform           | Build status                                                                                                                                                                    |
| --------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AppVeyor        | Windows     | [![Build status](https://ci.appveyor.com/api/projects/status/kj3n6vwax0ds9k2k?svg=true)](https://ci.appveyor.com/project/unknownv2/corehook)                                    |
| Azure Pipelines | Linux, Windows     | [![Build Status](https://unknowndev.visualstudio.com/CoreHook/_apis/build/status/CoreHook/CoreHook)](https://unknowndev.visualstudio.com/CoreHook/_build/latest?definitionId=2) |
| Travis CI       | Linux              | [![Build Status](https://travis-ci.com/unknownv2/CoreHook.svg?branch=master)](https://travis-ci.com/unknownv2/CoreHook)                                                         |

## Features
* Intercept public API functions such as `CreateFile`
* Intercept internal functions by address or [name if symbol files are available](#windows-symbol-support)
* Supports NuGet package references for the plugin libraries 
* Supports multiple architectures for the plugins

For more information, [see the wiki](https://github.com/unknownv2/CoreHook/wiki).

## Supported Platforms

CoreHook supports application function call interception on various architectures running [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x). [Linux and macOS support is also planned](https://github.com/unknownv2/CoreHook/wiki/Linux-and-macOS-Support).


| Architecture  | Operating System      |
| ------------- |:---------------------:|
| x86           | Windows               |
| x64           | Windows               |
| ARM           | Windows 10 IoT Core   |

## Tested Platforms

| Operating System    | Architecture(s)       |
| ------------------  |:---------------------:|
| Windows 7 SP1       | x86, x64              |
| Windows 8.1         | x86, x64              |
| Windows 10 (Win32)  | x86, x64, ARM         |
| Windows 10 (UWP)    | x86, x64              |
| Windows Server 2008 | x86, x64              |
| Windows Server 2012 | x86, x64              |
| Windows Server 2016 | x86, x64              |
| Windows Server 2019 | x86, x64              |

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)

## Examples

 * [FileMonitor - Universal Windows Platform (UWP)](examples/Uwp/CoreHook.Uwp.FileMonitor/) 
 * [FileMonitor - Windows Desktop Applications (Win32)](examples/Win32/CoreHook.FileMonitor)

## Plugin Examples

* [More examples for the plugins can be found in this repository](https://github.com/unknownv2/corehook-plugins)

## Usage

### Windows

If you are building the CoreHook project (for example, with `dotnet build`) and not publishing it, you must setup the project configuration as described below.

#### Project Configuration

The project provides two options for configuring the runtime: 

1. A local configuration file named `CoreHook.CoreLoad.runtimeconfig.json`
(which is located next to `CoreHook.CoreLoad.dll` assembly in the CoreHook output directory) to initialize CoreCLR. 
2. A global configuration file named `dotnet.runtimeconfig.json`.

The host module will first attempt to use the local configuration file, then it will check for the the global configuration file and use that if it exists, and finally it will use the directory of the `CoreHook.CoreLoad.dll` assembly for resolving dependencies.

The `runtimeconfig` file must contain the framework information for hosting .NET Core in the target application.
When you build any .NET Core application, these files are generated to the output directory. [For more information on the
configuration options, see here](https://github.com/dotnet/cli/blob/master/Documentation/specs/runtime-configuration-file.md).

You can use the `CoreHook.FileMonitor.runtimeconfig.json` and `CoreHook.FileMonitor.runtimeconfig.dev.json` files found in your build output directory as references for creating the global or local configuration files.

The runtime configuration file should look like the one below, where `additionalProbingPaths` contains file paths the host module can check for additional dependencies. This guide assumes you have installed the `2.2` runtime or SDK for both x86 and x64 architectures.

**Notice: Either replace `<user>` with your local computer user name or modify the paths to point to where your NuGet packages are installed. Take a look at `CoreHook.FileMonitor.runtimeconfig.dev.json` in the output directory to find your paths.**

```json
{
  "runtimeOptions": {
    "tfm": "netcoreapp2.2",
    "framework": {
      "name": "Microsoft.NETCore.App",
      "version": "2.2.0"
    },
    "additionalProbingPaths": [
      "C:\\Users\\<user>\\.dotnet\\store\\|arch|\\|tfm|",
      "C:\\Users\\<user>\\.nuget\\packages",
      "C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder"
    ]
  }
}
```

#### Local Configuration

To use a local configuration, create a file with the contents described above called `CoreHook.CoreLoad.runtimeconfig.json` and save it to the project output directory where `CoreHook.CoreLoad.dll` is located.

#### Global Configuration

To use a global configuration, first create a `dotnet.runtimeconfig.json` file with the contents described above and save it to a folder. This will be the global configuration file the project uses to initialize the runtime in the target processs. In this example, our file is saved at `C:\CoreHook\dotnet.runtimeconfig.json`.

Set the environment variables for the `x86` and `x64` applications to the directory of the runtime configuration file. This allows you to have different configuration files for `32-bit` and `64-bit` applications. 

For example (if you saved the file another installation directory or drive, make sure to use that path instead):

 * Set `CORE_ROOT_32` to `C:\CoreHook` for `32-bit` applications.
 
 * Set `CORE_ROOT_64` to `C:\CoreHook` for `64-bit` applications.


```ps
setx CORE_ROOT_64 "C:\CoreHook"
setx CORE_ROOT_32 "C:\CoreHook"
```

Or set them for the current command prompt session with:

```
set CORE_ROOT_64=C:\CoreHook
set CORE_ROOT_32=C:\CoreHook
```

Then, you can either open the `CoreHook` solution in `Visual Studio` or run `dotnet build` to build the library and the examples.

#### Installing Dependencies

Build or download the binary releases from [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking) and [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host). You can use the [download-deps](/download-deps.cmd) script, which downloads the latest binary releases to a folder called `deps` in the root of the project. 
Place the `coreload32.dll (X86, ARM)` and/or `coreload64.dll (X64, ARM64)` binaries in the output directory of your program. Then, place the `corehook32.dll (X86, ARM)` and/or `corehook64.dll (X64, ARM64)` binaries in the same output directory. These are all of the required files for using the examples above. 

You can then start the program you built above.

### Windows 10 UWP

You can get the Application User Model Id (AUMID) required for launching UWP apps for the FileMonitor example with [this script:](
https://docs.microsoft.com/en-us/windows/configuration/find-the-application-user-model-id-of-an-installed-app)
```ps
$installedapps = get-AppxPackage

$aumidList = @()
foreach ($app in $installedapps)
{
    foreach ($id in (Get-AppxPackageManifest $app).package.applications.application.id)
    {
        $aumidList += $app.packagefamilyname + "!" + $id
    }
}

$aumidList
```
 You can print the list using the `$aumidList` variable.

 **Notes:** There is currently no way to set the proper access control on our pipes on the .NET Core platform and the issue is [being tracked here](https://github.com/dotnet/corefx/issues/31190) so we use P/Invoke to call `kernel32.dll!CreateNamedPipe` directly.

### Windows 10 IoT Core (ARM)
**There is currently no ARM .NET Core SDK that runs on Windows 10 IoT Core, so you must publish the application from a platform with an SDK and copy it to your IoT Core device. [You can read more about the publishing process here.](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)**

For `Windows 10 IoT Core`, you can publish the application by running the `publish.ps1` [PowerShell script](#publishing-script).

```ps
.\publish -example win32 -runtime win-arm
```

Make sure to also copy the `coreload32.dll` and the `corehook32.dll` to the directory of the program. For example, the application directory structure should look like this:

```
[+]Publish\win32\win-arm\
    [+]Hook\
        ...
        [-] CoreHook.FileMonitor.Hook.deps.json
        [-] CoreHook.FileMonitor.Hook.dll
        ...
    ...    
    [-] CoreHook.FileMonitor.dll
    [-] CoreHook.FileMonitor.exe
    [-] corehook32.dll
    [-] coreload32.dll
    ...
```


You can then copy the folder to your device and start the `CoreHook.FileMonitor.exe` program.

### Publishing Script

The PowerShell script `publish.ps1` allows you to publish the [examples](/examples) as self-contained executables. The default configuration is `Release` and the output will be in the `Publish` directory, created in the same location as the publishing script.

```ps
.\publish -example [uwp|win32] -runtime [Runtime IDentifier] -configuration [Debug|Release]
```

**You can find a list of Runtime IDentifiers [here](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)**.

For example, the command

```ps
.\publish -example win32 -runtime win10-arm
```

will create a folder called `Publish/win32/win10-arm/` containing the `CoreHook.FileMonitor` example.


### Windows Symbol Support

CoreHook supports symbol name lookup from PDBs to get function addresses with the use of `LocalHook.GetProcAddress`. For symbol lookup to work, you must either place the PDB file in the directory of the target program you are hooking or set the environment variable `_NT_SYMBOL_PATH` to a symbol server. [You can read more about Windows symbol support from the Microsoft documentation here.](https://docs.microsoft.com/en-us/windows/desktop/dxtecharts/debugging-with-symbols#using-the-microsoft-symbol-server)

**Important:** To use the complete symbol lookup, you need to have both `dbghelp.dll` (provides the symbol lookup APIs) and `symsrv.dll` (provides the symbol server lookup) and in your [DLL search path](https://docs.microsoft.com/en-us/windows/desktop/dlls/dynamic-link-library-search-order). You can add these files to the directory of your target program or add them to your path. You can get both DLLs by installing the [***Debugging Tools for Windows***](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/).

Example locations where you can find `dbghelp.dll` and `symsrv.dll` are:

* **%PROGRAMFILES(X86)%\Windows Kits\10\Debuggers\x86** (For 32-bit applications)
* **%PROGRAMFILES(X86)%\Windows Kits\10\Debuggers\x64** (For 64-bit applications)

An example of what you can set the environment variable `_NT_SYMBOL_PATH` to is:

```
srv*C:\SymbolCache*https://msdl.microsoft.com/downloads/symbols
```

The `C:\SymbolCache` folder is a local cache directory where symbol files can be stored or downloaded to. When Windows needs to retrieve a PDB for a DLL, it can download them from `https://msdl.microsoft.com/downloads/symbols` and store them in a folder for use by a debugger.

You can confirm that symbol support is properly configured by running the [symbols tests](tests/CoreHook.Tests/Windows/SymbolsTest.cs).

## Contributing

Any contributions are all welcome! If you find any problems or want to add features, don't hesitate to open a new issue or create a pull request.

## License

Licensed under the [MIT](LICENSE) License.

## [Credits](/CREDITS.md)