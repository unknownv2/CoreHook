using System.IO;

namespace CoreHook.BinaryInjection.Loader.Serializer
{
    public class BinaryLoaderSerializer : IBinarySerializer
    {
        public IBinaryLoaderArguments Arguments { get; set; }
        public IBinaryLoaderConfig Config { get; }

        public BinaryLoaderSerializer(IBinaryLoaderConfig config)
        {
            Config = config;
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Serialize information about the serialized class 
                // Data that is passed to the remote function
                writer.Write(Arguments.Verbose);
                // Padding for reserved data to align structure to 8 bytes
                writer.Write(new byte[7]);
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(Arguments.PayloadFileName, Config.MaxPathLength, Config.PathEncoding));
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(Arguments.CoreRootPath, Config.MaxPathLength, Config.PathEncoding));
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(Arguments.CoreLibrariesPath ?? string.Empty, Config.MaxPathLength, Config.PathEncoding));

                return ms.ToArray();
            }
        }
    }
}