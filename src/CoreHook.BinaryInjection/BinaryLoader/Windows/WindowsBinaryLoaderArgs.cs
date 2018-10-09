using System;
using System.Collections.Generic;
using System.IO;
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
                CoreLibrariesPath = BinaryLoaderArgs.GetPathArray(args.CoreLibrariesPath ?? string.Empty, PathLength, Encoding)
             };
        }
    }

    public interface IBinaryLoaderConfig
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }
    }

    public class WindowsBinaryLoaderConfig : IBinaryLoaderConfig
    {
        public int MaxPathLength { get => 260; }
        public Encoding PathEncoding { get => Encoding.Unicode; }
    }

    public class BinaryLoaderSerializer : IBinarySerializer
    {
        public BinaryLoaderArgs Arguments { get; set; }
        public IBinaryLoaderConfig Config { get; }

        public BinaryLoaderSerializer(IBinaryLoaderConfig config)
        {
            Config = config;
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    // serialize information about the serialized class 
                    // data that is passed to the remote function
                    writer.Write(Arguments.Verbose);
                    writer.Write(Arguments.WaitForDebugger);
                    writer.Write(Arguments.StartAssembly);
                    // padding for reserved data to align structure to 8 bytes
                    writer.Write(new byte[5]);
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.PayloadFileName, Config.MaxPathLength, Config.PathEncoding));
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.CoreRootPath, Config.MaxPathLength, Config.PathEncoding));
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.CoreLibrariesPath ?? string.Empty, Config.MaxPathLength, Config.PathEncoding));
                }
                return ms.ToArray();
            }
        }
    }

}
