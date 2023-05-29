
using CoreHook.BinaryInjection.Loader;

using System;

namespace CoreHook.BinaryInjection.RemoteInjection;

public class RemoteInjector : IDisposable
{
    private readonly int _targetProcessId;
    private readonly ManagedProcess _managedProcess;
    private readonly AssemblyDelegate _assmbDelegate;


    public RemoteInjector(int targetProcessId, AssemblyDelegate assmbDelegate)
    {
        _targetProcessId = targetProcessId;
        _managedProcess = new ManagedProcess(targetProcessId);
        _assmbDelegate = assmbDelegate;
    }

    /// <summary>
    /// Start CoreCLR and execute a .NET assembly in a target process.
    /// </summary>
    /// <param name="localProcessId">Process ID of the process communicating with the target process.</param>
    /// <param name="targetProcessId">The process ID of the process to inject the .NET assembly into.</param>
    /// <param name="remoteInjectorConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
    /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
    /// <param name="passThruArguments">Arguments passed to the .NET hooking plugin once it is loaded in the target process.</param>
    public void Inject(RemoteInjectorConfiguration remoteInjectorConfig, params object?[] passThruArguments)
    {
        if (string.IsNullOrWhiteSpace(remoteInjectorConfig.InjectionPipeName))
        {
            throw new ArgumentException("Invalid injection pipe name");
        }

        InjectionHelper.BeginInjection(_targetProcessId);

        using (InjectionHelper.CreateServer(remoteInjectorConfig.InjectionPipeName, remoteInjectorConfig.PipePlatform))
        {
            try
            {
                foreach(var lib in remoteInjectorConfig.Libraries)
                {
                    _managedProcess.InjectModule(lib);
                }

                //// Load the CoreCLR hosting module in the remote process.
                _managedProcess.InjectModule(remoteInjectorConfig.HostLibrary);

                ////TODO: replace HostLibrary with Libraries? Allowing to consider hosting as a caller-option (and allowing additional native lib loading?)
                //assemblyLoader.LoadModule(remoteInjectorConfig.HostLibrary.Replace($"CoreHook.NativeHost{(process.Is64Bit() ? 64 : 32)}.dll", "nethost.dll"));
                //assemblyLoader.LoadModule(remoteInjectorConfig.HostLibrary);

                //// Load the function detour module into remote process.
                //assemblyLoader.LoadModule(remoteInjectorConfig.DetourLibrary);

                // Initialize CoreCLR in the remote process using the native CoreCLR hosting module.
                var hostFuncArgs = remoteInjectorConfig.NetHostStartArguments;
                _managedProcess.CreateThread(remoteInjectorConfig.HostLibrary, ClrHostConfiguration.ClrStartFunction, ref hostFuncArgs);

                //TODO: should probably use a first call to generate the unmanaged delegate function pointer, then issue the call when needed by the initiator?
                // Or make one call for each (startClr first, inject and run then, from the caller)
                var args = new ManagedFunctionArguments(_assmbDelegate, remoteInjectorConfig.PayLoad);
                _managedProcess.CreateThread(remoteInjectorConfig.HostLibrary, ClrHostConfiguration.ClrExecuteManagedFunction, ref args, false);

                InjectionHelper.WaitForInjection(_targetProcessId, 120000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                InjectionHelper.EndInjection(_targetProcessId);
            }
        }
    }

    ~RemoteInjector()
    {
        Dispose();
    }

    public void Dispose()
    {
        _managedProcess.Dispose();
    }
}
