using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreHook.BinaryInjection
{
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
                    // Serialize information about the serialized class 
                    // Data that is passed to the remote function
                    writer.Write(Arguments.Verbose);
                    writer.Write(Arguments.WaitForDebugger);
                    // Padding for reserved data to align structure to 8 bytes
                    writer.Write(new byte[6]);
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.PayloadFileName, Config.MaxPathLength, Config.PathEncoding));
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.CoreRootPath, Config.MaxPathLength, Config.PathEncoding));
                    writer.Write(BinaryLoaderArgs.GetPathArray(Arguments.CoreLibrariesPath ?? string.Empty, Config.MaxPathLength, Config.PathEncoding));
                }
                return ms.ToArray();
            }
        }
    }

}
