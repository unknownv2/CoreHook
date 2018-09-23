using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.ManagedHook.Remote
{
    public class ProcessCreationConfig : ICreateProcessConfig
    {
        public string ExecutablePath { get; set; }
        public string CommandLine { get; set; }
        public uint ProcessCreationFlags { get; set; }
    }
    public class RemoteHookingConfig : ICoreHookConfig, ICoreRunConfig, ICoreLoadConfig
    {   
        public string DetourLibrary { get; set; }
        public string PayloadLibrary { get; set;  }
        public string HostLibrary { get; set; }
        public bool VerboseLog { get; set; }
        public bool WaitForDebugger { get; set; }
        public bool StartAssembly { get; set; }

        public string CLRBootstrapLibrary { get; set; }
        public string CoreCLRPath { get; set; }
        public string CoreCLRLibrariesPath { get; set; }
    }
    public interface ICreateProcessConfig
    {
        /// <summary>
        /// Filepath to an executable program on the system that is to be launched.
        /// </summary>
        string ExecutablePath { get; set;  }
        /// <summary>
        /// Arguments to be passed to the executable program when it is started
        /// </summary>
        string CommandLine { get; set; }
        /// <summary>
        /// Attributes and options passed to the process creation function call.
        /// </summary>
        uint ProcessCreationFlags { get; set; }
    }
    public interface ICoreHookConfig
    {
        /// <summary>
        /// Library that implements function intercept exports for the LocalHook class
        /// such as DetourInstallHook.
        /// </summary>
        string DetourLibrary { get; set; }
        /// <summary>
        /// .NET library that is loaded and executed inside the target process
        /// by the boostrap library after starting the .NET Core runtime.
        /// </summary>
        string PayloadLibrary { get; set; }
    }
    public interface ICoreLoadConfig
    {
        /// <summary>
        /// Library that resolves dependencies and passes arguments to
        /// the .NET payload Assembly.
        /// </summary>
        string CLRBootstrapLibrary { get; set; }
        /// <summary>
        /// Directory path which contains the main CoreCLR modules for hosting the runtime
        /// such as CoreCLR and clrjit libraries.
        /// </summary>
        string CoreCLRPath { get; set; }
        /// <summary>
        /// Directory path which contains the CoreCLR Assembly reference
        /// runtime libraries such as System.Private.CoreLib.dll.
        /// </summary>
        string CoreCLRLibrariesPath { get; set; }
    }
    public interface ICoreRunConfig
    {        
        /// <summary>
        /// Library that initializes the .NET Core runtime (CoreCLR) and allows
        /// loading and executing .NET Assemblies.
        /// </summary>
        string HostLibrary{ get; set; }
        /// <summary>
        /// Option to enable the logging module inside the HostLibrary.
        /// </summary>
        bool VerboseLog { get; set; }
        /// <summary>
        /// Option to enable waiting for a debugger to attach before any 
        /// .NET Assemblies are loaded by the HostLibrary.
        /// </summary>
        bool WaitForDebugger { get; set; }
        /// <summary>
        /// Option to immediately execute an .NET Assembly after initializing
        /// the .NET Core runtime.
        /// </summary>
        bool StartAssembly { get; set; }
    }
}
