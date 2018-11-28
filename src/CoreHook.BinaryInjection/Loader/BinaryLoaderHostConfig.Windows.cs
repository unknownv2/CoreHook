
namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class BinaryLoaderHostConfig
    {
        public static string ClrStartFunction
            => "StartCoreCLR";
        public static string ClrExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }
}
