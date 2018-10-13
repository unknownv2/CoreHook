using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CoreHook.Unmanaged
{
    public static class ProcessExtensions
    {
        public static bool BringToFront(this Process process)
        {
            if (IsActiveWindow(process))
            {
                return true;
            }

            var hWindow = process.MainWindowHandle;

            var style = NativeMethods.GetWindowLong(hWindow, NativeMethods.GWL_STYLE);

            if ((style & NativeMethods.WS_MINIMIZE) != 0)
            {
                NativeMethods.ShowWindow(hWindow, NativeMethods.ShowWindowCommand.Normal);
            }

            return NativeMethods.BringWindowToTop(process.MainWindowHandle);
        }

        public static bool IsActiveWindow(this Process process)
        {
            if (process.HasExited)
            {
                return false;
            }

            var activeWindow = NativeMethods.GetForegroundWindow();

            if (activeWindow.Equals(IntPtr.Zero))
            {
                return false;
            }

            uint pid;

            NativeMethods.GetWindowThreadProcessId(activeWindow, out pid);

            return Marshal.GetLastWin32Error() == 0 && pid == process.Id;
        }

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

                SafeProcessHandle handle = NativeMethods.OpenProcess(
                     NativeMethods.ProcessAccessFlags.QueryInformation,
                     false,
                     process.Id
                 );

                if (handle == null)
                {
                    throw new Win32Exception();
                }

                using (handle)
                {
                    bool ret;

                    if (!NativeMethods.IsWow64Process(handle, out ret))
                    {
                        throw new Win32Exception();
                    }

                    return !ret;
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Cannot determine process architecture");
            }
        }
    }
}
