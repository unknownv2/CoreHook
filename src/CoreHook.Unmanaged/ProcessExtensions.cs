using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Unmanaged
{
    public static class ProcessExtensions
    {
        public static bool Is64Bit(this Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Environment.Is64BitOperatingSystem;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Environment.Is64BitOperatingSystem)
                {
                    return false;
                }
         
                SafeProcessHandle processHandle = Interop.Kernel32.OpenProcess(
                    Interop.Advapi32.ProcessOptions.PROCESS_QUERY_INFORMATION,
                    false,
                    process.Id);

                if (processHandle.IsInvalid)
                {
                    throw new Win32Exception("Failed to open process handle");
                }

                using (processHandle)
                {
                    bool processIsWow64 = false;
                    if (!Interop.Kernel32.IsWow64Process(processHandle, ref processIsWow64))
                    {
                        throw new Win32Exception("Cannot determine process architecture");
                    }

                    return !processIsWow64;
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Cannot determine process architecture");
            }
        }
    }
}
