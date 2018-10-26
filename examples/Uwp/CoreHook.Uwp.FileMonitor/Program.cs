using System;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using CoreHook.FileMonitor.Service;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.Remote;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.Memory;

namespace CoreHook.Uwp.FileMonitor
{
    class Program
    {
        /// <summary>
        /// The pipe name over which the FileMonitor RPC service communicates over between processes.
        /// </summary>
        private const string CoreHookPipeName = "UwpCoreHook";
        /// <summary>
        /// The directory containing the CoreHook modules to be loaded in processes.
        /// </summary>
        private const string HookLibraryDirName = "Hook";
        /// <summary>
        /// The library injected to be injected the target processed and executed
        /// using it's 'Run' Method.
        /// </summary>
        private const string HookLibraryName = "CoreHook.Uwp.FileMonitor.Hook.dll";
        /// <summary>
        /// The name of the pipe used for notifying the host process
        /// if the hooking plugin has been loaded successfully loaded in
        /// the target process or not. 
        /// </summary>
        private const string InjectionPipeName = "UwpCoreHookInjection";
        /// <summary>
        /// Enable verbose logging to the console for the CoreCLR host module corerundll.
        /// </summary>
        private const bool HostVerboseLog = false;
        /// <summary>
        /// Wait for a debugger to attach to the target process before running any .NET assemblies.
        /// </summary>
        private const bool HostWaitForDebugger = false;
        /// <summary>
        /// Class that handles creating a named pipe server for communicating with the target process.
        /// </summary>
        private static IPipePlatform PipePlatform = new Pipe.PipePlatform();

        private static void Main(string[] args)
        {
            int targetPID = 0;
            string targetApp = string.Empty;

            // Load the parameter
            while ((args.Length != 1) || !int.TryParse(args[0], out targetPID))
            {
                if (targetPID > 0)
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

            string injectionLibrary = Path.Combine(currentDir, HookLibraryDirName, HookLibraryName);

            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMonitor injection dll");
                return;
            }

            // Grant read+execute permissions on the binary files
            // we are injecting into the UWP application
            GrantAllAppPkgsAccessToDir(currentDir);
            GrantAllAppPkgsAccessToDir(Path.GetDirectoryName(injectionLibrary));

            // Start the target process and begin dll loading
            if (!string.IsNullOrEmpty(targetApp))
            {
                targetPID = LaunchAppxPackageForPid(targetApp);
            }

            // Inject the FileMonitor.Hook dll into the process
            InjectDllIntoTarget(targetPID, injectionLibrary);

            // Start the RPC server for handling requests from the hooked app
            StartListener();
        }
        /// <summary>
        /// Inject and load the CoreHook hooking module <paramref name="injectionLibrary"/>
        /// in the existing created process referenced by <paramref name="processId"/>.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="injectionLibrary"></param>
        /// <param name="injectionPipeName"></param>
        private static void InjectDllIntoTarget(
            int processId, 
            string injectionLibrary,
            string injectionPipeName = InjectionPipeName)
        {
            if (Examples.Common.Utilities.GetCoreLoadPaths(
                ProcessHelper.GetProcessById(processId).Is64Bit(),
                out string coreRunDll, out string coreLibrariesPath, 
                out string coreRootPath, out string coreLoadDll,
                out string corehookPath))
            {
                // Make sure the native dll modules can be accessed by the UWP application
                GrantAllAppPkgsAccessToFile(coreRunDll);
                GrantAllAppPkgsAccessToFile(corehookPath);

                RemoteHooking.Inject(
                    processId,
                    new RemoteHookingConfig
                    {
                        CoreCLRPath = coreRootPath,
                        CoreCLRLibrariesPath = coreLibrariesPath,
                        CLRBootstrapLibrary = coreLoadDll,
                        DetourLibrary = corehookPath,
                        HostLibrary = coreRunDll,
                        InjectionPipeName = injectionPipeName,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = HostVerboseLog,
                        WaitForDebugger = HostWaitForDebugger
                    },
                    PipePlatform,
                    CoreHookPipeName);
            }
        }

        /// <summary>
        /// Create an RPC server that is called by the RPC client started in a target process.
        /// </summary>
        private static void StartListener()
        {
            var session = new FileMonitorSessionFeature();

            Examples.Common.RpcService.CreateRpcService(
                  CoreHookPipeName,
                  PipePlatform,
                  session,
                  typeof(FileMonitorService),
                  async (context, next) =>
                  {
                      Console.WriteLine("> {0}", context.Request);
                      await next();
                      Console.WriteLine("< {0}", context.Response);
                  });

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();

            session.StopServer();
        }

        /// <summary>
        /// Grant ALL_APPLICATION_PACKAGES permissions to binary and 
        /// configuration files in <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath">Directory containing application files.</param>
        private static void GrantAllAppPkgsAccessToDir(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            GrantAllAppPkgsAccessToFolder(directoryPath);
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".json") || name.EndsWith(".dll")))
            {
                GrantFolderRecursive(filePath, directoryPath);
                GrantAllAppPkgsAccessToFile(filePath);
            }
        }

        /// <summary>
        /// Grant ALL_APPLICATION_PACKAGES permissions to the Symbol Cache directory <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath">A directory containing Windows symbols (.PDB files).</param>
        private static void GrantAllAppPkgsAccessToSymCacheDir(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            GrantAllAppPkgsAccessToFolder(directoryPath);

            foreach (var filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".pdb")))
            {
                GrantFolderRecursive(filePath, directoryPath);
                GrantAllAppPkgsAccessToFile(filePath);
            }
        }
        /// <summary>
        /// Grant ALL_APPLICATION_PACKAGES permissions to a directory and its subdirectories.
        /// </summary>
        /// <param name="path">The path of the directory to grant permissions to.</param>
        /// <param name="rootDirectory">The root marking when to stop granting permissions if reached.</param>
        private static void GrantFolderRecursive(string path, string rootDirectory)
        {
            while((path = Path.GetDirectoryName(path)) != rootDirectory)
            {
                GrantAllAppPkgsAccessToFolder(path);
            }
        }

        /// <summary>
        /// Grant ALL_APPLICATION_PACKAGES permissions to a file at <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The file to be granted ALL_APPLICATION_PACKAGES permissions.</param>
        private static void GrantAllAppPkgsAccessToFile(string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                FileSecurity acl = fileInfo.GetAccessControl();

                var rule = new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), 
                               FileSystemRights.ReadAndExecute, AccessControlType.Allow);
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
        private static void GrantAllAppPkgsAccessToFolder(string folderPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                DirectorySecurity acl = dirInfo.GetAccessControl(AccessControlSections.Access);

                var rule = new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
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
        /// <returns></returns>
        private static int LaunchAppxPackageForPid(string appName)
        {
            var appActiveManager = new ApplicationActivationManager();
            uint pid;

            try
            {
                // PackageFamilyName + {Applications.Application.Id}, inside AppxManifest.xml
                appActiveManager.ActivateApplication(appName, null, ActivateOptions.None, out pid);

                return (int)pid;
            }
            catch
            {
                return 0;
            }
        }
    }
}
