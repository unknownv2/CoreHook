using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LinuxBinaryLoaderArgs
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] PayloadFileName;

        [FieldOffset(4104)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] CoreRootPath;

        [FieldOffset(8200)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] CoreLibrariesPath;
    }
}