using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WriteProcessMemory")]
        internal static extern bool WriteProcessMemory(
            SafeHandle processHandle,
            IntPtr baseAddress, 
            byte[] buffer, 
            int size,
            out IntPtr numberOfBytesWritten);
    }
}
