using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public interface IBinaryLoaderConfiguration
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }
    }
}
