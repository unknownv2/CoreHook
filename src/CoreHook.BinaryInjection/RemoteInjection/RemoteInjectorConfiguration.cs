
namespace CoreHook.BinaryInjection.RemoteInjection
{
    public class RemoteInjectorConfiguration : NativeModulesConfiguration
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
        public string ClrBootstrapLibrary { get; set; }
        /// <summary>
        /// The name of the pipe used for notifying the host process
        /// when the hooking plugin has been successfully loaded in
        /// the target process or if the injection process failed.
        /// </summary>
        public string InjectionPipeName { get; set; }

        public RemoteInjectorConfiguration() { }

        public RemoteInjectorConfiguration(NativeModulesConfiguration nativeConfig)
        {
            ClrLibrariesPath = nativeConfig.ClrLibrariesPath;
            ClrRootPath = nativeConfig.ClrRootPath;
            HostLibrary = nativeConfig.HostLibrary;
            DetourLibrary = nativeConfig.DetourLibrary;
        }
    }
}
