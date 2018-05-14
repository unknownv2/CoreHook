# CoreHook

A library to hook unmanaged applications by hosting .NET Core and running managed code to extend an application's functionality (more information [here](https://github.com/dotnet/docs/blob/master/docs/core/tutorials/netcore-hosting.md)).  

Inspired and based on the great [EasyHook](https://github.com/EasyHook/EasyHook).

## Dependencies
* [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)
* [EasyHook](https://github.com/EasyHook/EasyHook)
* [CoreRun](https://github.com/unknownv2/CoreHook.Host)

Currently only supports 64-bit programs on [Windows](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x) and it works for Win32 and UWP apps. The plan is to get it working on other platforms supported by the .NET Core 2.x such as [Linux](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x) and [MacOS](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x).



**The library is still in development and a lot might be broken. Pull requests/contributions are all welcome!**