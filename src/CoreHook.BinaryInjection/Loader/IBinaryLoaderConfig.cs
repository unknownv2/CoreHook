using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IBinaryLoaderConfig
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }
    }
}
