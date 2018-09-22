using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.ManagedHook.Remote
{
    public class ProcessCreationConfig: ICreateProcessConfig
    {
        public string ExePath { get; set; }
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
        string ExePath { get; set;  }
        string CommandLine { get; set; }
        uint ProcessCreationFlags { get; set; }
    }
    public interface ICoreHookConfig
    {
        string DetourLibrary { get; set; }
        string PayloadLibrary { get; set; }
    }
    public interface ICoreLoadConfig
    {
        string CLRBootstrapLibrary { get; set; }
        string CoreCLRPath { get; set; }
        string CoreCLRLibrariesPath { get; set; }
    }
    public interface ICoreRunConfig
    {        
        string HostLibrary{ get; set; }
        bool VerboseLog { get; set; }
        bool WaitForDebugger { get; set; }
        bool StartAssembly { get; set; }
    }
}
