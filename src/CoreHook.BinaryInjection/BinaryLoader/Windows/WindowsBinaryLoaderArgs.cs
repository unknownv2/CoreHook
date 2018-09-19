using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Explicit)]
    public struct WindowsBinaryLoaderArgs
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength*2)]
        public byte[] PayloadFileName;

        [FieldOffset(528)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength*2)]
        public byte[] CoreRootPath;

        [FieldOffset(1048)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength*2)]
        public byte[] CoreLibrariesPath;

        private static Encoding Encoding = Encoding.Unicode;
        private const int PathLength = 260;

        public static WindowsBinaryLoaderArgs Create(BinaryLoaderArgs args)
        {
            return new WindowsBinaryLoaderArgs()
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
