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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] PayloadFileName;

        [FieldOffset(4104)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] CoreRootPath;

        [FieldOffset(8200)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] CoreLibrariesPath;

        private static Encoding Encoding = Encoding.ASCII;
        private const int PathLength = 4096;

        public static LinuxBinaryLoaderArgs Create(BinaryLoaderArgs args)
        {
            return new LinuxBinaryLoaderArgs()
            {
                Verbose = args.Verbose,
                WaitForDebugger = args.WaitForDebugger,
                StartAssembly = args.StartAssembly,
                PayloadFileName = BinaryLoaderArgs.GetPathArray(args.PayloadFileName, PathLength, Encoding),
                CoreRootPath = BinaryLoaderArgs.GetPathArray(args.CoreRootPath, PathLength, Encoding),
                CoreLibrariesPath = BinaryLoaderArgs.GetPathArray(args.CoreLibrariesPath, PathLength, Encoding)
            };
        }
    }
}