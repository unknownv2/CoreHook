using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
    public class BinaryLoaderArgs
    {
        public bool Verbose;

        public bool WaitForDebugger;

        public bool StartAssembly;

        public string PayloadFileName;

        public string CoreRootPath;

        public string CoreLibrariesPath;

        public Encoding Encoding;

        public int PathLength;

        public static byte[] GetPathArray(string path, int pathLength, Encoding encoding)
        {
            return encoding.GetBytes(path.PadRight(pathLength, '\0'));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BinaryLoaderArgs2
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

        public byte[] PayloadFileName;

        public byte[] CoreRootPath;

        public byte[] CoreLibrariesPath;
    }
}