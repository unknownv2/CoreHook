using CoreHook.BinaryInjection.Loader;
using CoreHook.IPC.Platform;

using System.Collections.Generic;

namespace CoreHook.BinaryInjection.RemoteInjection;

public class RemoteInjectorConfiguration
{
    public NetHostStartArguments NetHostStartArguments { get; set; } = new();

    /// <summary>
    /// .NET library that is loaded and executed inside the target process
    /// by the bootstrap library after starting the .NET Core runtime.
    /// </summary>
    public string PayloadLibrary { get; set; }

    /// <summary>
    /// Library that initializes the .NET Core runtime (CoreCLR) and allows
    /// loading and executing .NET Assemblies.
    /// </summary>
    public string HostLibrary { get; set; }

    /// <summary>
    /// The name of the pipe used for notifying the host process
    /// when the hooking plugin has been successfully loaded in
    /// the target process or if the injection process failed.
    /// </summary>
    public string InjectionPipeName { get; set; }

    public IList<string> Libraries { get; set; }

    public IPipePlatform PipePlatform { get; set; }
    
    public object PayLoad { get; set; }

    public RemoteInjectorConfiguration() { }
}
