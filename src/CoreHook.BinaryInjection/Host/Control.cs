using System.Diagnostics;
using CoreHook.Unmanaged;

namespace CoreHook.BinaryInjection
{
    public class Control
    {
        // Inject an assembly into a process
        private static string LoadAssemblyFunc = "LoadAssembly";
        public static void ExecuteAssemblyWithArgs(Process process, string module, BinaryLoaderArgs args)
        {
            process.Execute(module, LoadAssemblyFunc, Binary.StructToByteArray(args));
        }

        // Execute a function inside a library using the class name and function name
        private static string ExecAssemblyFunc = "ExecuteAssemblyFunction";
        public static void LoadAssemblyWithArgs(Process process, string module, FunctionCallArgs args)
        {
            process.Execute(module, ExecAssemblyFunc, Binary.StructToByteArray(args));
        }
    }
}
