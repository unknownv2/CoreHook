# CoreHook

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/unknownv2/CoreHook/blob/master/LICENSE)

A library that simplifies intercepting application function calls using managed code by [hosting](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md) the .NET Core runtime. 

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook). 

## Contents

- [Build Status](#build-status)
- [Features](#features)
- [Supported Platforms](#supported-platforms)
- [Tested Platforms](#tested-platforms)
- [Dependencies](#dependencies)
- [Examples](#examples)
- [Usage](#usage)
    - [Windows](##windows)
- [Contributing](#contributing)
- [Credits](#credits)
- [Licenses](#licenses)
## Build status

| Build server    | Platform           | Build status                                                                                                                                                                    |
| --------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AppVeyor        | Linux, Windows     | [![Build status](https://ci.appveyor.com/api/projects/status/kj3n6vwax0ds9k2k?svg=true)](https://ci.appveyor.com/project/unknownv2/corehook)                                    |
| Azure Pipelines | Linux              | [![Build Status](https://unknowndev.visualstudio.com/CoreHook/_apis/build/status/CoreHook/CoreHook)](https://unknowndev.visualstudio.com/CoreHook/_build/latest?definitionId=2) |
| Travis CI       | Linux              | [![Build Status](https://travis-ci.com/unknownv2/CoreHook.svg?branch=master)](https://travis-ci.com/unknownv2/CoreHook)                                                         |


## Features
* Intercept public API functions such as [kernel32.dll!CreateFile](https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-createfilew) on Windows or [libc!open](http://man7.org/linux/man-pages/man2/open.2.html) on Unix systems
* Intercept internal functions by address or [name if symbol files are available](#windows-symbol-support)
* Write libraries for intercepting API calls that can be ran on multiple architectures without any changes

For more information, [see the wiki](https://github.com/unknownv2/CoreHook/wiki).

## Supported Platforms

CoreHook supports application function call interception on various architectures running [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x), [macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x), and [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x).

| Architecture  | Operating System      | Working    |
| ------------- |:---------------------:|:----------:|
| x86           | Windows               | Yes        |
| x64           | Linux, macOS, Windows | Yes        |
| ARM32         | Linux, Windows        | Windows    |
| ARM64         | Linux, Windows        | WIP        |

## Tested Platforms

| Operating System    | Architecture(s)       |
| ------------------  |:---------------------:|
| macOS High Sierra   | x64                   |
| Ubuntu 14           | x64                   |
| Ubuntu 16           | x64                   |
| Windows 7 SP1       | x86, x64              |
| Windows 8.1         | x86, x64              |
| Windows 10 (Win32)  | x86, x64, ARM32       |
| Windows 10 (UWP)    | x86, x64              |
| Windows Server 2008 | x86, x64              |
| Windows Server 2012 | x86, x64              |
| Windows Server 2016 | x86, x64              |

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)
* [CoreHook.ProcessInjection](https://github.com/unknownv2/CoreHook.ProcessInjection)
* [CoreHook.UnixHook](https://github.com/unknownv2/CoreHook.UnixHook)

## Examples

 * [FileMonitor - Linux and macOS (Unix)](examples/Unix/CoreHook.Unix.FileMonitor/)
 * [FileMonitor - Universal Windows Platform (UWP)](examples/UWP/CoreHook.UWP.FileMonitor/) 
 * [FileMonitor - Windows Desktop Applications (Win32)](examples/Win32/CoreHook.FileMonitor)

## Usage

### Windows

First, set the environment variables for the `x86` and `x64` applications to the installation folder of your desired dotnet runtime. 

Using the `.NET Core 2.1` runtime as an example (validate the paths if you have another installation directory or drive):

 * Set `CORE_ROOT_32` to `C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\2.1.5` for `32-bit` applications.
 
 * Set `CORE_ROOT_64` to `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.5` for `64-bit` applications.

You can run the following commmands to set the environment variables for your user account, and they will be set for the next command prompt or the next program you open, such as Visual Studio:

```ps
setx CORE_ROOT_64 "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.5"
setx CORE_ROOT_32 "C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\2.1.5"
```

Or set them for the current command prompt session with:

```ps
set CORE_ROOT_64=C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.5
set CORE_ROOT_32=C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\2.1.5
```

Then open the `CoreHook` solution in `Visual Studio` and you can build the examples, either `CoreHook.FileMonitor` or `CoreHook.UWP.FileMonitor`.

Finally, build or download the binary releases (in ZIP files) from [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking) and [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host). Place the `corerundll32.dll (X86, ARM)` and/or `corerundll64.dll (X64, ARM64)` binaries in the output directory of your program. Then, place the `corehook32.dll (X86, ARM)` and/or `corehook64.dll (X64, ARM64)` binaries in the same output directory. These are all of the required files for using the examples above. 

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

### Windows 10 IoT Core (ARM32)
**There is currently no ARM32 SDK for .NET Core, so you must publish the application and copy it to your device. [You can read more about the publishing process here.](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)**

For `Windows 10 IoT Core`, you can publish the application by running the `publish.ps1` [PowerShell script](#publishing-script).

```ps
.\publish -example win32 -runtime win-arm
```

Make sure to also copy the `corerundll32.dll` and the `corehook32.dll` to the directory of the program. For example, the application directory structure should look like this:

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
    [-] corerundll32.dll
    ...
```


You can then copy the folder to your device and start the `CoreHook.FileMonitor.exe` program.

### Publishing Script

The PowerShell script `publish.ps1` allows you to publish the [examples](/examples) as self-contained executables. The default configuration is `Release` and the output will be in the `Publish` directory, created in the same location as the publishing script.

```ps
.\publish -example [unix|uwp|win32] -runtime [Runtime IDentifier] -configuration [Debug|Release]
```

**You can find a list of Runtime IDentifiers [here](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)**.

For example, the command

```ps
.\publish -example win32 -runtime win10-arm
```

will create a folder called `Publish/win32/win10-arm/` containing the `CoreHook.FileMonitor` example.

```ps
.\publish -example uwp -runtime win10-arm64
```
will create a folder called `Publish/uwp/win10-arm64/` containing the `CoreHook.UWP.FileMonitor` example.


### Windows Symbol Support

CoreHook supports symbol name lookup from PDBs to get function addresses with the use of `LocalHook.GetProcAddress`. For symbol lookup to work, you must either place the PDB file in the directory of the target program you are hooking or set the environment variable `_NT_SYMBOL_PATH` to a symbol server. [You can read more about Windows symbol support from the Microsoft documentation here.](https://docs.microsoft.com/en-us/windows/desktop/dxtecharts/debugging-with-symbols#using-the-microsoft-symbol-server)

**Important:** To use the complete symbol lookup, you need to have both `dbghelp.dll` (provides the symbol lookup APIs) and `symsrv.dll` (provides the symbol server lookup) and in your [DLL search path](https://docs.microsoft.com/en-us/windows/desktop/dlls/dynamic-link-library-search-order). You can add these files to the directory of your target program or add them to your path. You can get both DLLs by installing the [***Debugging Tools for Windows***](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/).

Example locations where you can find `dbghelp.dll` and `symsrv.dll` are:

* *C:\Program Files (x86)\Windows Kits\10\Debuggers\x86* (For 32-bit applications)
* *C:\Program Files (x86)\Windows Kits\10\Debuggers\x64* (For 64-bit applications)


An example of what you can set the environment variable `_NT_SYMBOL_PATH` to is:

```
srv*C:\SymbolCache*https://msdl.microsoft.com/downloads/symbols
```

The `C:\SymbolCache` folder is a local cache directory where symbol files can be stored or downloaded to. When Windows needs to retrieve a PDB for a DLL, it can download them from `https://msdl.microsoft.com/downloads/symbols` and store them in a folder for use by a debugger.

You can confirm that symbol support is properly configured by running the [symbols tests](tests/CoreHook.Tests/Windows/SymbolsTest.cs).

## Contributing

Any contributions are all welcome! If you find any problems or want to add features, don't hesitate to open a new issue or create a pull request.

## Credits

A lot of this project is based on the work of others who were willing to share their knowledge.

* [Christoph Husse and Justin Stenning](https://github.com/EasyHook/EasyHook) - The original developers of the EasyHook project which this one would not be possible without. A large amount of code in CoreHook is borrowed from their great work, going from C# all the way to assembly code. 
* [Nate McMaster](https://github.com/natemcmaster) - For the build and publishing PowerShell scripts and other great tools he has created, such as the [.NET Core Plugins](https://github.com/natemcmaster/DotNetCorePlugins).
* [dotnet Team](https://github.com/dotnet) - For great strides in innovation and constantly working on improving the .NET Core framework. Code from the open-source .NET Core framework was used in this project and is really helpful in the developmnet process. 

## Licenses

[EasyHook - MIT](https://github.com/EasyHook/EasyHook/blob/master/LICENSE)