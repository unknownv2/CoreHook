namespace CoreHook.BinaryInjection.Loader;

internal static class ClrHostConfiguration
{
    internal static string ClrStartFunction => "StartCoreCLR";
    internal static string ClrExecuteManagedFunction => "ExecuteAssemblyFunction";
}
