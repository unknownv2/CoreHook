
namespace CoreHook.BinaryInjection.Loader.Configuration
{
    internal sealed class ClrHostConfiguration
    {
        internal static string ClrStartFunction => "StartCoreCLR";
        internal static string ClrExecuteManagedFunction => "ExecuteAssemblyFunction";
    }
}
