
namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class BinaryLoaderHostConfiguration
    {
        public static string ClrStartFunction
            => "StartCoreCLR";
        public static string ClrExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }
}
