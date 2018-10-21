using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ReadProcessMemory(
            SafeProcessHandle processHandle,
            IntPtr address,
            [Out] byte[] buffer,
            UIntPtr size,
            out UIntPtr numberOfBytesRead);
    }
}
