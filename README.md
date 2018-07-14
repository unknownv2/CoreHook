# CoreHook

A library to hook unmanaged applications by hosting .NET Core and running managed code to extend an application's functionality (more information [here](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md)) on various architectures running  [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x), [macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x), and [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x).  

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook).

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)
* [CoreHook.ProcessInjection](https://github.com/unknownv2/CoreHook.ProcessInjection)
* [CoreHook.UnixHook](https://github.com/unknownv2/CoreHook.UnixHook)
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
 * [FileMonitor - Windows Desktop Applications (Win32)](Examples/CoreHook.FileMonitor)

## Usage

### Windows

First, set the  environment variables `CORE_LIBRARIES` and `CORE_ROOT` to the installation folder of your desired dotnet runtime. For example, the default installation folder for .NET Core 2.1 is at `C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.1.0`. So you set both `CORE_LIBRARIES` and `CORE_ROOT` to point to that folder. 

Then open the `CoreHook` solution `(.sln file)` in Visual Studio and you can build examples, either `CoreHook.FileMonitor` or `CoreHook.UWP.FileMonitor`.

Finally, build or download the binary releases (in ZIP files) from [CoreHook.Hooking](https://github.com/unknownv2/CoreHook.Hooking) and [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host). Place the `CoreRunDLL32.dll (X86, ARM)` and/or `CoreRunDLL64.dll (X64, ARM64)` binaries in the output directory of your program. Then, also place the `corehook32.dll (X86, ARM)` and/or `corehook64.dll (X64, ARM64)` binaries in the same output directory. These are all of the required files for using the examples above. 

You can then start the program you built above.

For `Windows 10 IoT (ARM)`, you will need to open a command prompt `cmd` and go to the `CoreHook.FileMonitor` directory and run `dotnet publish -r win-arm`. Then go to the `CoreHook.FileMonitor.Hook` directory and run `dotnet publish -r win-arm` again. Inside the `Build` folder, you will find a `win-arm\publish` folder containing `CoreHook.FileMonitor.exe`. Copy the contents of the `publish` folder to your device and then copy the contents of the `win-arm\publish` folder containing `CoreHook.FileMonitor.Hook.dll` inside a folder named `netstandard2.0` in the same directory the original `CoreHook.FileMonitor.exe` is copied to. Make sure to also copy the `CoreRunDLL32.dll` and the `corehook32.dll` to the directory of the program. For example, the structure should look like this:

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
    [-] CoreRunDLL32.dll
    ...
```

You can then start the `CoreHook.FileMonitor.exe` program on your ARM device.

## Notes on UWP Usage

 There is currently no way to set the proper access control on our pipes on the .NET Core platform and the issue is [being tracked here](https://github.com/dotnet/corefx/issues/30170) so we use PInvoke to call `kernel32.dll!CreateNamedPipe` directly.

**The library is still in development and a lot might be broken. Pull requests/contributions are all welcome!**