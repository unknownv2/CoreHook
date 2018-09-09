# CoreHook

A library to intercept function calls in applications and extend their functionality with managed code by [hosting](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md) the .NET Core runtime inside applications on various architectures running [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x), [macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x), and [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x).

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook).

## Build status

| Build server | Platform           | Build status                             |
| ------------ | ------------------ | ---------------------------------------- |
| AppVeyor     | Linux, Windows     | [![Build status](https://ci.appveyor.com/api/projects/status/kj3n6vwax0ds9k2k?svg=true)](https://ci.appveyor.com/project/unknownv2/corehook) |


## Features
* Intercept public API functions and internal functions by address or [name if symbol files are available](#windows-symbol-support)
* Write libraries for intercepting API calls that can be ran on multiple architectures without any changes

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)
* [CoreHook.ProcessInjection](https://github.com/unknownv2/CoreHook.ProcessInjection)
* [CoreHook.UnixHook](https://github.com/unknownv2/CoreHook.UnixHook)
* [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [JsonRpc (For Examples Only)](https://github.com/CXuesong/JsonRpc.Standard)


## Supported Platforms

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

## Examples

 * [FileMonitor - Linux and macOS (Unix)](examples/Unix/CoreHook.Unix.FileMonitor/)
 * [FileMonitor - Universal Windows Platform (UWP)](examples/UWP/CoreHook.UWP.FileMonitor/) 
 * [FileMonitor - Windows Desktop Applications (Win32)](examples/Win32/CoreHook.FileMonitor)

## Usage

### Windows

First, set the environment variables for the `x86` and `x64` applications to the installation folder of your desired dotnet runtime. 

Using the `.NET Core 2.1` runtime as an example (validate the paths if you have another installation directory or drive):

 * Set `CORE_LIBRARIES_32` and `CORE_ROOT_32` to `C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\2.1.3` for `32-bit(x86)` applications.
 
 * Set `CORE_LIBRARIES_64` and `CORE_ROOT_64` to `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.3` for `64-bit(x64)` applications.


Then open the `CoreHook` solution `(.sln file)` in Visual Studio and you can build the examples, either `CoreHook.FileMonitor` or `CoreHook.UWP.FileMonitor`.

Finally, build or download the binary releases (in ZIP files) from [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking) and [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host). Place the `corerundll32.dll (X86, ARM)` and/or `corerundll64.dll (X64, ARM64)` binaries in the output directory of your program. Then, place the `corehook32.dll (X86, ARM)` and/or `corehook64.dll (X64, ARM64)` binaries in the same output directory. These are all of the required files for using the examples above. 

You can then start the program you built above.

### Windows 10 IoT Core (ARM32)
**There is currently no ARM32 SDK for .NET Core, so you must publish the application and copy it to your device. [You can read more about the publishing process here.](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)**

For `Windows 10 IoT Core`, you can publish the application by running the `publish.ps1` [PowerShell script](#publishing-script).

```
.\publish -example win32 -runtime win-arm
```

Make sure to also copy the `corerundll32.dll` and the `corehook32.dll` to the directory of the program. For example, the structure should look like this:

```
[+]Corehook.FileMonitor.PublishFolder\
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

## Publishing Script

The PowerShell script `publish.ps1` allows you to publish the [examples](/examples) as self-contained executables. The default configuration is `Release` and the output will be in the `Publish` directory, created in the same location as the publishing script.

```
.\publish -example [unix|uwp|win32] -runtime [Runtime IDentifier] -configuration [Debug|Release]
```

**You can find a list of Runtime IDentifiers [here](docs.microsoft.com/en-us/dotnet/core/rid-catalog)**.

For example, the command

```
.\publish -example win32 -runtime win10-arm
```

will create a folder called `Publish/win32/win10-arm/` containing the `CoreHook.FileMonitor` example.

```
.\publish -example uwp -runtime win10-arm64
```
will create a folder called `Publish/uwp/win10-arm64/` containing the `CoreHook.UWP.FileMonitor` example.


### Windows Symbol Support

CoreHook supports symbol name lookup from PDBs to get function addresses with the use of `LocalHook.GetProcAddress`. For symbol lookup to work, you must either place the PDB file in the directory of the target program you are hooking or set the environment variable `_NT_SYMBOL_PATH` to a symbol server. [You can read more about Windows symbol support from the Microsoft documentation here.](https://docs.microsoft.com/en-us/windows/desktop/dxtecharts/debugging-with-symbols#using-the-microsoft-symbol-server)

**Important: To use the symbol server lookup, you need to have the `symsrv.dll` file in the same directory as `dbghelp.dll` (which provides the symbol lookup APIs). You can add these files to the directory of your target program or add them to your path. You can get `symsrv.dll` by installing [***Debugging Tools for Windows***, which you can find here](https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/).

. Example locations where you can find `symsrv.dll` are:

* *C:\Program Files (x86)\Windows Kits\10\Debuggers\x86* (For 32-bit applications)
* *C:\Program Files (x86)\Windows Kits\10\Debuggers\x64* (For 64-bit applications)


An example of what you can set the environment variable `_NT_SYMBOL_PATH` to is:

```
srv*C:\SymbolCache*https://msdl.microsoft.com/downloads/symbols
```

The `C:\SymbolCache` folder is a local cache directory where symbol files can be stored or downloaded to. When Windows needs to retrieve a PDB for a DLL, it can download them from `https://msdl.microsoft.com/downloads/symbols` and store them in a folder for use by a debugger.

You can test symbol support by running the `DetourInternalFunction{XX}` [tests](test/CoreHook.Tests/LocalHookTests.Windows.cs).

### Notes on Windows UWP Usage

 There is currently no way to set the proper access control on our pipes on the .NET Core platform and the issue is [being tracked here](https://github.com/dotnet/corefx/issues/31190) so we use P/Invoke to call `kernel32.dll!CreateNamedPipe` directly.

## Contributing

Any contributions are all welcome! If you find any problems or want to add features, don't hesitate to open a new issue or create a pull request.

## Licenses

[EasyHook - MIT](https://github.com/EasyHook/EasyHook/blob/master/LICENSE)