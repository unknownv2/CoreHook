using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct BinaryLoaderArgs
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U1)]
        public bool Verbose;

        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.U1)]
        public bool WaitForDebugger;

        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.U1)]
        public bool StartAssembly;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
        public byte[] PayloadFileName;

        [FieldOffset(528)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
        public byte[] CoreRootPath;

        [FieldOffset(1048)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
        public byte[] CoreLibrariesPath;
    }
}