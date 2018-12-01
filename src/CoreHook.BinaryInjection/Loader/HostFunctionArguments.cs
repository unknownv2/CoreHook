using System.IO;
using CoreHook.BinaryInjection.Loader.Serializers;

namespace CoreHook.BinaryInjection.Loader
{
    public class HostFunctionArguments : IBinarySerializer
    {
        public IHostArguments LoaderArguments { get; set; }
        public IPathConfiguration LoaderConfig { get; }

        public HostFunctionArguments(IPathConfiguration loaderConfig, IHostArguments loaderArguments)
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
                writer.Write(PathArgumentsHelper.GetPathArray(LoaderArguments.PayloadFileName, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));
                writer.Write(PathArgumentsHelper.GetPathArray(LoaderArguments.CoreRootPath, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));
                writer.Write(PathArgumentsHelper.GetPathArray(LoaderArguments.CoreLibrariesPath ?? string.Empty, LoaderConfig.MaxPathLength, LoaderConfig.PathEncoding));

                return ms.ToArray();
            }
        }
    }
}