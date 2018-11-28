
namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class BinaryLoaderHostConfig
    {
        public static string CoreCLRStartFunction
            => "StartCoreCLR";
        public static string CoreCLRExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }
}
