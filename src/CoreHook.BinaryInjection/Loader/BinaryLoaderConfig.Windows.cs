using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class BinaryLoaderConfig : IBinaryLoaderConfig
    {
        public int MaxPathLength => 260; 
        public Encoding PathEncoding => Encoding.Unicode;
    }
}
