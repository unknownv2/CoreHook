using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal partial class Psapi
    {
        [DllImport(Libraries.Psapi, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EnumProcessModulesEx(
            SafeProcessHandle processHandle,
            [Out] IntPtr module,
            uint moduleArraySize,
            out uint moduleArraySizeNeeded,
            uint filterFlag);

        [DllImport(Libraries.Psapi, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EnumProcessModulesEx(
            SafeProcessHandle processHandle,
            [Out] IntPtr module,
            uint moduleArraySize,
            ref int moduleArraySizeNeeded,
            uint filterFlag);

        internal partial class ModuleFilterFlags
        {
            internal const uint Default = 0x00;
            internal const uint Bit32 = 0x01;
            internal const uint Bit64 = 0x02;
            internal const uint All = 0x03;
        }
    }
}
