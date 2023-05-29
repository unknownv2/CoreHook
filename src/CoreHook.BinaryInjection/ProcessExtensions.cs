
using Microsoft.Win32.SafeHandles;

using System;
using System.ComponentModel;
using System.Diagnostics;

using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace CoreHook.BinaryInjection;

public static class ProcessExtensions
{
    public static bool Is64Bit(this Process process)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Environment.Is64BitOperatingSystem;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Platform not supported for detecting process architecture.");
        }

        if (!Environment.Is64BitOperatingSystem)
        {
            return false;
        }

        SafeFileHandle processHandle = NativeMethods.OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, (uint)process.Id);

        if (processHandle.IsInvalid)
        {
            throw new Win32Exception("Failed to open process query handle.");
        }

        using (processHandle)
        {
            BOOL processIsWow64 = false;
            if (!NativeMethods.IsWow64Process(processHandle, out processIsWow64))
            {
                throw new Win32Exception("Determining process architecture with IsWow64Process failed.");
            }

            return !processIsWow64;
        }
    }
}
