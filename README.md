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

## Using

First copy the 3 following DLL's into the same directory as `CoreHook.CoreLoad.dll` ( you can find them in your NuGet packages directory, ex: `C:\Users\{UserName}\.nuget\packages` or the NuGet Fallback Folder, ex: `C:\Program Files\dotnet\sdk\NuGetFallbackFolder`):

```
Microsoft.Extensions.DependencyModel.dll
Microsoft.DotNet.PlatformAbstractions.dll
Newtonsoft.Json.dll
```

Make sure that any references for the dll's in the other examples are the same version as the ones you place in the `CoreHook.CoreLoad.dll` directory. For example, in the `FileMonitor` examples mentioned above, if `JsonRpc.Standard.dll` (used by `CoreHook.FileMonitor.Hook.dll` after it is injected into the target process) references `Newtonsoft.Json.dll` version `11.0.2` then that should be the same version you copy to the directory containing `CoreHook.CoreLoad.dll`.

## Notes on UWP Usage

 There is currently no way to set the proper access control on our pipes on the .NET Core platform and the issue is [being tracked here](https://github.com/dotnet/corefx/issues/30170) so we use PInvoke to call `kernel32.dll!CreateNamedPipe` directly.

**The library is still in development and a lot might be broken. Pull requests/contributions are all welcome!**