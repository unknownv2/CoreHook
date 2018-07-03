using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DotnetAssemblyFunctionCall
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] coreRunLib;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] binaryLoaderFunctionName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] assemblyCallFunctionName;
        public MacOSBinaryLoaderArgs binaryLoaderArgs;
        public LinuxFunctionCallArgs assemblyFunctionCall;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] PayloadFileName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] CoreRootPath;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] CoreLibrariesPath;
    }
}