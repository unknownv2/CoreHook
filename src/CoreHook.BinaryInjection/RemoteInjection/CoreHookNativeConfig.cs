using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.RemoteInjection
{
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
}
