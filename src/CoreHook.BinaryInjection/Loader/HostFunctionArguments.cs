using System.IO;
using CoreHook.BinaryInjection.Loader.Configuration;
using CoreHook.BinaryInjection.Loader.Serialization;

namespace CoreHook.BinaryInjection.Loader
{
    public class HostFunctionArguments : ISerializableObject
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
                // Write arguments passed to the CLR hosting function.
                writer.Write(LoaderArguments.Verbose);
                // Padding for reserved data to align structure to 8 bytes.
                writer.Write(new byte[7]);
                // Write the paths used for hosting the CLR.
                writer.Write(PathArgumentsHelper.GetPathArray(LoaderArguments.PayloadFileName, LoaderConfig));
                writer.Write(PathArgumentsHelper.GetPathArray(LoaderArguments.CoreRootPath, LoaderConfig));

                return ms.ToArray();
            }
        }
    }
}