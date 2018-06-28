# CoreHook

A library to hook unmanaged applications by hosting .NET Core and running managed code to extend an application's functionality (more information [here](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md)).  

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook).

## Dependencies

* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [CoreHook.Host](https://github.com/unknownv2/CoreHook.Host)
* [CoreHook.ProcessInjection](https://github.com/unknownv2/CoreHook.ProcessInjection)
* [CoreHook.UnixHook](https://github.com/unknownv2/CoreHook.UnixHook)
* [EasyHook](https://github.com/EasyHook/EasyHook)
* [JsonRpc (For Examples Only)](https://github.com/CXuesong/JsonRpc.Standard) 

Currently, CoreHook supports Win32 and UWP apps (x86, x64) on [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x). It also works on [macOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x) and [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x) for x64 applications.

So far it has been tested on Ubuntu 14, Ubuntu 16, macOS High Sierra, and Windows 10.

## Examples

 * [FileMonitor - Linux and macOS (Unix)](Examples/Unix/CoreHook.Unix.FileMonitor/)
 * [FileMonitor - Uiversal Windows Platform (UWP)](Examples/UWP/CoreHook.UWP.FileMonitor/) 
 * [FileMonitor - Windows Desktop Applications (Win32)](Examples/CoreHook.FileMonitor)


 The UWP sample program requires .NET Framework version 4.6.1 to support setting permissions on the DLL's that are loaded in the target process and .NET Standard 2.0 ([more information here](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)).

**The library is still in development and a lot might be broken. Pull requests/contributions are all welcome!**