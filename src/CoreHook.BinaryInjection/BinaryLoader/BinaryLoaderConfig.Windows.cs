using System.Text;

namespace CoreHook.BinaryInjection.BinaryLoader
{
    public sealed partial class BinaryLoaderHostConfig
    {
        public static string CoreCLRStartFunction
            => "StartCoreCLR";
        public static string CoreCLRExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }

    public interface IBinaryLoaderConfig
    {
        int MaxPathLength { get; }
        Encoding PathEncoding { get; }
    }

    public sealed partial class BinaryLoaderConfig : IBinaryLoaderConfig
    {
        public int MaxPathLength => 260; 
        public Encoding PathEncoding => Encoding.Unicode;
    }
}
