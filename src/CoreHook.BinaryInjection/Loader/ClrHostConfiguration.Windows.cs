
namespace CoreHook.BinaryInjection.Loader
{
    public sealed partial class ClrHostConfiguration
    {
        public static string ClrStartFunction
            => "StartCoreCLR";
        public static string ClrExecuteManagedFunction
            => "ExecuteAssemblyFunction";
    }
}
