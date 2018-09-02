# CoreHook

A library to intercept function calls in applications and extend their functionality with managed code by [hosting](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md) the .NET Core runtime inside applications on various architectures running [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x), [macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x), and [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x).

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook).

**The library is still in development and a lot might be broken. Pull requests/contributions are all welcome!**

## Features
* Intercept public API functions and internal functions [if symbol files are available](#windows-symbol-support)
* Write libraries for intercepting API calls that can be ran on multiple architectures without any changes

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)
* [CoreHook.ProcessInjection](https://github.com/unknownv2/CoreHook.ProcessInjection)
* [CoreHook.UnixHook](https://github.com/unknownv2/CoreHook.UnixHook)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
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

 * [FileMonitor - Linux and macOS (Unix)](Examples/Unix/CoreHook.Unix.FileMonitor/)
 * [FileMonitor - Universal Windows Platform (UWP)](Examples/UWP/CoreHook.UWP.FileMonitor/) 
 * [FileMonitor - Windows Desktop Applications (Win32)](Examples/Win32/CoreHook.FileMonitor)

## Usage

### Windows

First, set the environment variables for the `x86` and `x64` applications to the installation folder of your desired dotnet runtime. 

Using the `.NET Core 2.1` runtime as an example (validate the paths if you have another installation directory or drive):

 * Set `CORE_LIBRARIES_32` and `CORE_ROOT_32` to `C:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\2.1.3` for `32-bit(x86)` applications.
 
 * Set `CORE_LIBRARIES_64` and `CORE_ROOT_64` to `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.3` for `64-bit(x64)` applications.


Then open the `CoreHook` solution `(.sln file)` in Visual Studio and you can build examples, either `CoreHook.FileMonitor` or `CoreHook.UWP.FileMonitor`.

Finally, build or download the binary releases (in ZIP files) from [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking) and [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host). Place the `corerundll32.dll (X86, ARM)` and/or `corerundll64.dll (X64, ARM64)` binaries in the output directory of your program. Then, also place the `corehook32.dll (X86, ARM)` and/or `corehook64.dll (X64, ARM64)` binaries in the same output directory. These are all of the required files for using the examples above. 

You can then start the program you built above.

### Windows 10 IoT Core (ARM32)
**There is currently no ARM32 SDK for .NET Core, so you must publish the application and copy it to your device. [You can read more about the publishing process here.](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md)**

For `Windows 10 IoT Core`, you will need to open a command prompt `cmd` and go to the `CoreHook.FileMonitor` directory and run `dotnet publish -r win-arm`. Then go to the `CoreHook.FileMonitor.Hook` directory and run `dotnet publish -r win-arm` again. Inside the `Build` folder, you will find a `win-arm\publish` folder containing `CoreHook.FileMonitor.exe`. Copy the contents of the `publish` folder to your device and then copy the contents of the `win-arm\publish` folder containing `CoreHook.FileMonitor.Hook.dll` inside a folder named `netstandard2.0` in the same directory the original `CoreHook.FileMonitor.exe` is copied to. Make sure to also copy the `corerundll32.dll` and the `corehook32.dll` to the directory of the program. For example, the structure should look like this:

```
[+]Corehook.FileMonitor.PublishFolder\
    [+]netstandard2.0\
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
You can then start the `CoreHook.FileMonitor.exe` program on your ARM device.

### Windows Symbol Support

CoreHook supports symbol name lookup from PDBs to get function addresses with the use of `LocalHook.GetProcAddress`. For symbol lookup to work, you must either place the PDB file in the directory of the target program you are hooking or set the environment variable `_NT_SYMBOL_PATH` to a symbol server. [You can read more about Windows symbol support from the Microsoft documentation here.](https://docs.microsoft.com/en-us/windows/desktop/dxtecharts/debugging-with-symbols#using-the-microsoft-symbol-server)

**Important: To use the symbol server lookup, you need to have the `symsrv.dll` file in the same directory as `dbghelp.dll` (which provides the symbol lookup APIs). You can add these files to the directory of your target program or add them to your path. You can find symsrv.dll in your Visual Studio directory or by installing a Windows SDK. You can also download them from [here](https://github.com/DarthTon/Blackbone/tree/master/DIA), but they could be outdated.**

Example locations where you can find `symsrv.dll` are:

* *C:\Program Files (x86)\Microsoft Visual Studio\2017\Product_Version\Common7\IDE* (where Product_Version is Community, Enterprise, etc...)
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


## Licenses

[EasyHook - MIT](https://github.com/EasyHook/EasyHook/blob/master/LICENSE)