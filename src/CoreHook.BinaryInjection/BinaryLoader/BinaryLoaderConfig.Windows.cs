using System.Text;

namespace CoreHook.BinaryInjection.BinaryLoader
{
    public interface IBinaryLoaderConfig
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }

    }
    public interface IBinaryLoaderHostConfig
    {
        string CoreCLRStartFunction { get; }
        string CoreCLRExecuteManagedFunction { get; }
    }

    public sealed partial class BinaryLoaderHostConfig : IBinaryLoaderHostConfig
    {
        public string CoreCLRStartFunction
            => "StartCoreCLR";
        public string CoreCLRExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }

    public sealed partial class BinaryLoaderConfig : IBinaryLoaderConfig
    {
        public int MaxPathLength => 260; 
        public Encoding PathEncoding => Encoding.Unicode;
    }
}
