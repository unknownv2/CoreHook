using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(
            SafeProcessHandle processHandle,
            IntPtr address,
            UIntPtr size,
            uint allocationType,
            uint protect);

        /*
        internal partial class AllocationType
        {
            internal const uint Commit = 0x1000;
            internal const uint Reserve = 0x2000;
            internal const uint Decommit = 0x4000;
            internal const uint Release = 0x8000;
            internal const uint Reset = 0x80000;
            internal const uint Physical = 0x400000;
            internal const uint TopDown = 0x100000;
            internal const uint WriteWatch = 0x200000;
            internal const uint LargePages = 0x20000000;
        }

        internal partial class MemoryProtection
        {
            internal const uint Execute = 0x10;
            internal const uint ExecuteRead = 0x20;
            internal const uint ExecuteReadWrite = 0x40;
            internal const uint ExecuteWriteCopy = 0x80;
            internal const uint NoAccess = 0x01;
            internal const uint ReadOnly = 0x02;
            internal const uint ReadWrite = 0x04;
            internal const uint WriteCopy = 0x08;
            internal const uint GuardModifierflag = 0x100;
            internal const uint NoCacheModifierflag = 0x200;
            internal const uint WriteCombineModifierflag = 0x400;
        }*/
    }
}
