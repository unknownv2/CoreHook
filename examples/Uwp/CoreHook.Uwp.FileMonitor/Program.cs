using CoreHook.FileMonitor.Service;
using CoreHook.HookDefinition;
using CoreHook.IPC.Platform;

using JsonRpc.Standard.Server;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CoreHook.Uwp.FileMonitor;

class Program
{
    /// <summary>
    /// The library to be injected into the target process and executed
    /// using it's 'Run' Method.
    /// </summary>
    private const string HookLibraryName = "CoreHook.Uwp.FileMonitor.Hook.dll";

    /// <summary>
    /// The name of the communication pipe that will be used for this program
    /// </summary>
    private const string PipeName = "FileMonitorUwpHookPipe";
    
    /// <summary>
    /// Class that handles creating a named pipe server for communicating with the target process.
    /// </summary>
    private static readonly IPipePlatform PipePlatform = new Pipe.PipePlatform();

    /// <summary>
    /// Security Identifier representing ALL_APPLICATION_PACKAGES permission.
    /// </summary>
    private static readonly SecurityIdentifier AllAppPackagesSid = new SecurityIdentifier("S-1-15-2-1");

    private static void Main(string[] args)
    {
        int targetProcessId = 0;
        string targetApp = string.Empty;

        // Get the process to hook by Application User Model Id for launching or process id for attaching.
        while ((args.Length != 1) || !int.TryParse(args[0], out targetProcessId))
        {
            if (targetProcessId > 0)
            {
                break;
            }
            if (args.Length != 1)
            {
                Console.WriteLine();
                Console.WriteLine("Usage: FileMonitor %PID%");
                Console.WriteLine("   or: FileMonitor AppUserModelId");
                Console.WriteLine();
                Console.Write("Please enter a process Id or the App Id to launch: ");

                args = new string[]
                {
                    Console.ReadLine()
                };

                if (string.IsNullOrWhiteSpace(args[0]))
                {
                    return;
                }
            }
            else
            {
                targetApp = args[0];
                break;
            }
        }

        var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        string injectionLibrary = Path.Combine(currentDir, HookLibraryName);

        if (!File.Exists(injectionLibrary))
        {
            Console.WriteLine("Cannot find FileMonitor injection dll");
            return;
        }

        // Grant read+execute permissions on the binary files
        // we are injecting into the UWP application.
        GrantAllAppPackagesAccessToDir(currentDir);
        GrantAllAppPackagesAccessToDir(Path.GetDirectoryName(injectionLibrary));

        // Start the target application and begin plugin loading.
        if (!string.IsNullOrEmpty(targetApp))
        {
            targetProcessId = LaunchAppxPackage(targetApp);
        }

        // Inject the FileMonitor.Hook dll into the process.
        RemoteHook.InjectDllIntoTarget(targetProcessId, injectionLibrary, PipePlatform, true, PipeName);

        // Start the RPC server for handling requests from the hooked app.
        StartListener();
    }

    /// <summary>
    /// Create an RPC server that is called by the RPC client started in a target process.
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

    /// <summary>
    /// Grant ALL_APPLICATION_PACKAGES permissions to binary
    /// and configuration files in <paramref name="directoryPath"/>.
    /// </summary>
    /// <param name="directoryPath">Directory containing application files.</param>
    private static void GrantAllAppPackagesAccessToDir(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        GrantAllAppPackagesAccessToFolder(directoryPath);
        foreach (var filePath in Directory.GetFiles(directoryPath, "*.json|*.dll|*.pdb", SearchOption.AllDirectories))
        {
            GrantFolderRecursive(filePath, directoryPath);
            GrantAllAppPackagesAccessToFile(filePath);
        }
    }

    /// <summary>
    /// Grant ALL_APPLICATION_PACKAGES permissions to the Symbol Cache directory <paramref name="directoryPath"/>.
    /// </summary>
    /// <param name="directoryPath">A directory containing Windows symbols (.PDB files).</param>
    private static void GrantAllAppPackagesAccessToSymCacheDir(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        GrantAllAppPackagesAccessToFolder(directoryPath);

        foreach (var filePath in Directory.GetFiles(directoryPath, "*.pdb", SearchOption.AllDirectories))
        {
            GrantFolderRecursive(filePath, directoryPath);
            GrantAllAppPackagesAccessToFile(filePath);
        }
    }

    /// <summary>
    /// Grant ALL_APPLICATION_PACKAGES permissions to a directory and its subdirectories.
    /// </summary>
    /// <param name="path">The path of the directory to grant permissions to.</param>
    /// <param name="rootDirectory">The root marking when to stop granting permissions if reached.</param>
    private static void GrantFolderRecursive(string path, string rootDirectory)
    {
        while ((path = Path.GetDirectoryName(path)) != rootDirectory)
        {
            GrantAllAppPackagesAccessToFolder(path);
        }
    }

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

    /// <summary>
    /// Grant ALL_APPLICATION_PACKAGES permissions to a directory at <paramref name="folderPath"/>.
    /// </summary>
    /// <param name="folderPath">The directory to be granted ALL_APPLICATION_PACKAGES permissions.</param>
    private static void GrantAllAppPackagesAccessToFolder(string folderPath)
    {
        try
        {
            var dirInfo = new DirectoryInfo(folderPath);
            DirectorySecurity acl = dirInfo.GetAccessControl(AccessControlSections.Access);

            var rule = new FileSystemAccessRule(AllAppPackagesSid,
                           FileSystemRights.ReadAndExecute, AccessControlType.Allow);

            acl.SetAccessRule(rule);

            dirInfo.SetAccessControl(acl);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Launch a Universal Windows Platform (UWP) application on Windows 10.
    /// </summary>
    /// <param name="appName">The Application User Model Id (AUMID) to start.</param>
    /// <returns>The process ID of the application started or 0 if launching failed.</returns>
    private static int LaunchAppxPackage(string appName)
    {
        var appActiveManager = new ApplicationActivationManager();

        try
        {
            // PackageFamilyName + {Applications.Application.Id}, inside AppxManifest.xml
            appActiveManager.ActivateApplication(appName, null, ActivateOptions.None, out var processId);

            return (int)processId;
        }
        catch
        {
            return 0;
        }
    }
}
