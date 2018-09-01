using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DotnetAssemblyFunctionCall
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MacOSBinaryLoaderArgs.PathLength)]
        public byte[] coreRunLib;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMax)]
        public byte[] binaryLoaderFunctionName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FunctionNameMax)]
        public byte[] assemblyCallFunctionName;

        public MacOSBinaryLoaderArgs binaryLoaderArgs;
        public LinuxFunctionCallArgs assemblyFunctionCall;

        private const int FunctionNameMax = 256;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MacOSBinaryLoaderArgs
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool Verbose;
        [MarshalAs(UnmanagedType.U1)]
        public bool WaitForDebugger;
        [MarshalAs(UnmanagedType.U1)]
        public bool StartAssembly;
        [MarshalAs(UnmanagedType.U1)]
        public bool Reserved1;
        [MarshalAs(UnmanagedType.U1)]
        public bool Reserved2;
        [MarshalAs(UnmanagedType.U1)]
        public bool Reserved3;
        [MarshalAs(UnmanagedType.U1)]
        public bool Reserved4;
        [MarshalAs(UnmanagedType.U1)]
        public bool Reserved5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] PayloadFileName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] CoreRootPath;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PathLength)]
        public byte[] CoreLibrariesPath;

        private static Encoding Encoding = Encoding.ASCII;
        internal const int PathLength = 1024;

        public static MacOSBinaryLoaderArgs Create(BinaryLoaderArgs args)
        {
            return new MacOSBinaryLoaderArgs()
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