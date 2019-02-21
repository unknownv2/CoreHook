
namespace CoreHook.BinaryInjection.RemoteInjection.Configuration
{
    public class NativeModulesConfiguration
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
        /// Directory path which contains the folder with a `dotnet.runtimeconfig.json`
        /// containing properties for initializing CoreCLR.
        /// </summary>
        public string ClrRootPath { get; set; }
    }
}
