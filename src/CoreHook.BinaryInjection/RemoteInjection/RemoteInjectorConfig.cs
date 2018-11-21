
namespace CoreHook.BinaryInjection.RemoteInjection
{
    public class ProcessCreationConfig
    {
        /// <summary>
        /// Filepath to an executable program on the system that is to be launched.
        /// </summary>
        public string ExecutablePath { get; set; }
        /// <summary>
        /// Arguments to be passed to the executable program when it is started
        /// </summary>
        public string CommandLine { get; set; }
        /// <summary>
        /// Attributes and options passed to the process creation function call.
        /// </summary>
        public uint ProcessCreationFlags { get; set; }
    }
    public class CoreHookNativeConfig
    {
        /// <summary>
        /// Library that initializes the .NET Core runtime (CoreCLR) and allows
        /// loading and executing .NET Assemblies.
        /// </summary>
        public string HostLibrary { get; set; }
        /// <summary>
        /// Library that implements function intercept exports for the LocalHook class
        /// such as DetourInstallHook.
        /// </summary>
        public string DetourLibrary { get; set; }
        /// <summary>
        /// Directory path which contains the main CoreCLR modules for hosting the runtime
        /// such as CoreCLR and clrjit libraries.
        /// </summary>
        public string CoreCLRPath { get; set; }
        /// <summary>
        /// Directory path which contains the CoreCLR Assembly reference
        /// runtime libraries such as System.Private.CoreLib.dll.
        /// </summary>
        public string CoreCLRLibrariesPath { get; set; }
    }
    public class RemoteInjectorConfig : CoreHookNativeConfig
    {
        /// <summary>
        /// .NET library that is loaded and executed inside the target process
        /// by the bootstrap library after starting the .NET Core runtime.
        /// </summary>
        public string PayloadLibrary { get; set; }
        /// <summary>
        /// Option to enable the logging module inside the HostLibrary.
        /// </summary>
        public bool VerboseLog { get; set; }
        /// <summary>
        /// Library that resolves dependencies and passes arguments to
        /// the .NET payload Assembly.
        /// </summary>
        public string CLRBootstrapLibrary { get; set; }
        /// <summary>
        /// The name of the pipe used for notifying the host process
        /// when the hooking plugin has been successfully loaded in
        /// the target process or if the injection process failed.
        /// </summary>
        public string InjectionPipeName { get; set; }
    }
}
