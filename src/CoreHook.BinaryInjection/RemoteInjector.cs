using CoreHook.BinaryInjection.RemoteInjection;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

using System;

namespace CoreHook.BinaryInjection;

public class RemoteInjector : IDisposable
{
    private readonly int _targetProcessId;
    private readonly ManagedProcess _managedProcess;
    private readonly INamedPipe _server;

    public RemoteInjector(int targetProcessId, IPipePlatform pipePlatform, string injectionPipeName)
    {
        if (string.IsNullOrWhiteSpace(injectionPipeName))
        {
            throw new ArgumentException("Invalid injection pipe name");
        }

        _targetProcessId = targetProcessId;
        _managedProcess = new ManagedProcess(targetProcessId);

        _server = InjectionHelper.CreateServer(injectionPipeName, pipePlatform);
    }

    /// <summary>
    /// Start CoreCLR and execute a .NET assembly in a target process.
    /// </summary>
    /// <param name="localProcessId">Process ID of the process communicating with the target process.</param>
    /// <param name="targetProcessId">The process ID of the process to inject the .NET assembly into.</param>
    /// <param name="remoteInjectorConfig">Configuration settings for starting CoreCLR and executing .NET assemblies.</param>
    /// <param name="pipePlatform">Class for creating pipes for communication with the target process.</param>
    /// <param name="passThruArguments">Arguments passed to the .NET hooking plugin once it is loaded in the target process.</param>
    public void Inject<T>(string hostLibrary, string method, T arguments, bool waitForExit = true, params string[] libraries)
    {
        //TODO: useless when waitForExit == true?
        InjectionHelper.BeginInjection(_targetProcessId);

        try
        {
            foreach (var lib in libraries)
            {
                _managedProcess.InjectModule(lib);
            }

            _managedProcess.InjectModule(hostLibrary);

            _managedProcess.CreateThread(hostLibrary, method, ref arguments, waitForExit);

            if (!waitForExit)
            {
                //TODO: set back the timeout to 10000 or 20000
                InjectionHelper.WaitForInjection(_targetProcessId, 120000);
            }
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

    ~RemoteInjector()
    {
        Dispose();
    }

    public void Dispose()
    {
        _managedProcess.Dispose();
        _server.Dispose();
    }
}
