using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class BinaryLoaderConfiguration : IBinaryLoaderConfiguration
    {
        public int MaxPathLength => 260;
        public Encoding PathEncoding => Encoding.Unicode;
    }
}
