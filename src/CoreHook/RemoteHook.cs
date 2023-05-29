using CoreHook.BinaryInjection;
using CoreHook.BinaryInjection.RemoteInjection;
using CoreHook.IPC.Platform;

using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CoreHook;

/// <summary>
/// 
/// </summary>
public static class RemoteHook
{

    /// <summary>
    /// The pipe name the FileMonitor RPC service communicates over between processes.
    /// </summary>
    private const string? CoreHookPipeName = "CoreHook";
    /// <summary>
    /// The name of the pipe used for notifying the host process
    /// if the hooking plugin has been loaded successfully in
    /// the target process or if loading failed.
    /// </summary>
    private const string InjectionPipeName = "CoreHookInjection";

    /// <summary>
    /// The .NET Assembly class that loads the .NET plugin, resolves any references, and executes
    /// the IEntryPoint.Run method for that plugin.
    /// </summary>
    private static readonly AssemblyDelegate CoreHookLoaderDelegate = new("CoreHook.CoreLoad", "CoreHook.CoreLoad.PluginLoader", "Load", "CoreHook.CoreLoad.PluginLoader+LoadDelegate, CoreHook.CoreLoad");
    
    /// <summary>
    /// Check if a file path is valid, otherwise throw an exception.
    /// </summary>
    /// <param name="filePath">Path to a file or directory to validate.</param>
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"Invalid file path {filePath}");
        }
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File path {filePath} does not exist");
        }
    }

    /// <summary>
    /// Inject and load the CoreHook hooking module <paramref name="injectionLibrary"/>
    /// in the existing created process referenced by <paramref name="targetProcessId"/>.
    /// </summary>
    /// <param name="targetProcessId">The target process ID to inject and load plugin into.</param>
    /// <param name="hookLibrary"></param>
    /// <param name="pipePlaform"></param>
    /// <param name="verboseLog"></param>
    /// <param name="parameters"></param>
    public static void InjectDllIntoTarget(int targetProcessId, string hookLibrary, IPipePlatform pipePlaform, bool verboseLog = false, params object[] parameters)
    {
        ValidateFilePath(hookLibrary);

        var is64Bits = Process.GetProcessById(targetProcessId).Is64Bit();

        var (coreRootPath, coreLoadPath, coreRunPath, corehookPath, hostpath) = ModulesPathHelper.GetCoreLoadPaths(is64Bits);

        // Make sure the native dll modules can be accessed by the UWP application
        //GrantAllAppPackagesAccessToFile(coreRunPath);
        //GrantAllAppPackagesAccessToFile(corehookPath);
        
        var injectorConfig = new RemoteInjectorConfiguration()
        {
            HostLibrary = coreRunPath,
            PayloadLibrary = hookLibrary,
            PayLoad = new ManagedRemoteInfo(Environment.ProcessId, InjectionPipeName, Path.GetFullPath(hookLibrary), parameters),
            Libraries = new[] { hostpath, corehookPath },

            PipePlatform = pipePlaform,
            InjectionPipeName = InjectionPipeName,

            NetHostStartArguments = new(coreLoadPath, coreRootPath, verboseLog)
        };

        using var injector = new RemoteInjector(targetProcessId, CoreHookLoaderDelegate);
        injector.Inject(injectorConfig, CoreHookPipeName);
    }


    private static readonly SecurityIdentifier AllAppPackagesSid = new SecurityIdentifier("S-1-15-2-1");

    //TODO: move in a utility class with conditional compilation for UWP
    /// <summary>
    /// Grant ALL_APPLICATION_PACKAGES permissions to a file at <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The file to be granted ALL_APPLICATION_PACKAGES permissions.</param>
    private static void GrantAllAppPackagesAccessToFile(string fileName)
    {
        try
        {
            var fileInfo = new FileInfo(fileName);
            FileSecurity acl = fileInfo.GetAccessControl();

            var rule = new FileSystemAccessRule(AllAppPackagesSid, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
            acl.SetAccessRule(rule);

            fileInfo.SetAccessControl(acl);
        }
        catch
        {
        }
    }

}
