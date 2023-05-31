using CoreHook.FileMonitor.Service;
using CoreHook.HookDefinition;
using CoreHook.IPC.Platform;

using JsonRpc.Standard.Server;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreHook.FileMonitor;

class Program
{
    /// <summary>
    /// The library to be injected into the target process and executed using the EntryPoint's 'Run' Method.
    /// </summary>
    private const string HookLibraryName = "CoreHook.FileMonitor.Hook.dll";

    /// <summary>
    /// The name of the communication pipe that will be used for this program
    /// </summary>
    private const string PipeName = "FileMonitorHookPipe";

    /// <summary>
    /// Class that handles creating a named pipe server for communicating with the target process.
    /// </summary>
    private static readonly IPipePlatform PipePlatform = new PipePlatform();

    private static void Main(string[] args)
    {
        int targetProcessId = 0;
        string targetProgram = string.Empty;

        // Get the process to hook by file path for launching or process id for loading into.
        while ((args.Length != 1) || !ParseProcessId(args[0], out targetProcessId) || !FindOnPath(args[0]))
        {
            if (targetProcessId > 0)
            {
                break;
            }
            if (args.Length != 1 || !FindOnPath(args[0]))
            {
                Console.WriteLine();
                Console.WriteLine("Usage: FileMonitor %PID%");
                Console.WriteLine("   or: FileMonitor PathToExecutable");
                Console.WriteLine();
                Console.Write("Please enter a process Id or path to executable: ");

                args = new string[]
                {
                    Console.ReadLine()
                };

                if (string.IsNullOrEmpty(args[0]))
                {
                    return;
                }
            }
            else
            {
                targetProgram = args[0];
                break;
            }
        }

        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string injectionLibrary = Path.Combine(currentDir, HookLibraryName);

        // Start process
        if (!string.IsNullOrWhiteSpace(targetProgram))
        {
            targetProcessId = Process.Start(targetProgram)?.Id ?? throw new InvalidOperationException($"Failed to start the executable at {targetProgram}");
        }

        // Inject FileMonitor dll into process.
        RemoteHook.InjectDllIntoTarget(targetProcessId, injectionLibrary, PipePlatform, true, PipeName);

        // Start the RPC server for handling requests from the hooked program.
        StartListener();
    }

    /// <summary>
    /// Get an existing process ID by value or by name.
    /// </summary>
    /// <param name="targetProgram">The ID or name of a process to lookup.</param>
    /// <param name="processId">The ID of the process if found.</param>
    /// <returns>True if there is an existing process with the specified ID or name.</returns>
    private static bool ParseProcessId(string targetProgram, out int processId)
    {
        if (!int.TryParse(targetProgram, out processId))
        {
            var process = Process.GetProcessesByName(targetProgram).FirstOrDefault();
            if (process is not null)
            {
                processId = process.Id;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if an application exists on the path.
    /// </summary>
    /// <param name="targetProgram">The program name, such as "notepad.exe".</param>
    /// <returns>True of the program is found on the path.</returns>
    private static bool FindOnPath(string targetProgram)
    {
        // File is in current dir or path was fully specified
        if (File.Exists(targetProgram))
        {
            return true;
        }

        // File wasn't found and path wasn't absolute: stop here
        if (Path.IsPathRooted(targetProgram))
        {
            return false;
        }

        // Or check in the configured paths
        var path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path.Split(";").Any(pathDir => File.Exists(Path.Combine(pathDir, targetProgram)));
        }

        return false;
    }

    /// <summary>
    /// Create an RPC server that is called by the RPC client started in
    /// a target process.
    /// </summary>
    private static void StartListener()
    {
        var session = new FileMonitorSessionFeature();

        Examples.Common.RpcService<FileMonitorService>.CreateRpcService(PipeName, PipePlatform, session, AsyncHandler);

        Console.WriteLine("Press Enter to quit.");
        Console.ReadLine();

        session.StopServer();
    }

    private static async Task AsyncHandler(RequestContext context, Func<Task> next)
    {
        Console.WriteLine("> {0}", context.Request);
        await next();
        Console.WriteLine("< {0}", context.Response);
    }
}