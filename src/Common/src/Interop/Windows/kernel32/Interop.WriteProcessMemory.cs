using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WriteProcessMemory")]
        internal static extern bool WriteProcessMemory(
            SafeProcessHandle processHandle,
            IntPtr baseAddress, 
            byte[] buffer, 
            int size,
            out IntPtr numberOfBytesWritten);
    }
}
