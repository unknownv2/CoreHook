using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool VirtualFreeEx(
            SafeProcessHandle processHandle,
            IntPtr address,
            UIntPtr size,
            uint freeType);

        internal partial class FreeType
        {
            internal const uint Decommit = 0x4000;
            internal const uint Release = 0x8000;
        }
    }
}
