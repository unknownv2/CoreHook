using System.IO;

namespace CoreHook.BinaryInjection.Loader.Serializer
{
    public class BinaryLoaderSerializer : IBinarySerializer
    {
        public IBinaryLoaderArguments LoaderArguments { get; set; }
        public IBinaryLoaderConfiguration LoaderConfig { get; }

        public BinaryLoaderSerializer(IBinaryLoaderConfiguration loaderConfig, IBinaryLoaderArguments loaderArguments)
        {
            LoaderConfig = loaderConfig;
            LoaderArguments = loaderArguments;
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Data that is passed to the remote function
                writer.Write(LoaderArguments.Verbose);
                // Padding for reserved data to align structure to 8 bytes
                writer.Write(new byte[7]);
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(LoaderArguments.PayloadFileName, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(LoaderArguments.CoreRootPath, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));
                writer.Write(BinaryLoaderArgumentsHelper.GetPathArray(LoaderArguments.CoreLibrariesPath ?? string.Empty, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));

                return ms.ToArray();
            }
        }
    }
}