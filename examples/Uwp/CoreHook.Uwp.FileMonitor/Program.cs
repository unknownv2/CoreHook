using System;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using CoreHook.BinaryInjection.RemoteInjection;
using CoreHook.BinaryInjection.ProcessUtils;
using CoreHook.FileMonitor.Service;
using CoreHook.IPC.Platform;
using CoreHook.Memory.Processes;

namespace CoreHook.Uwp.FileMonitor
{
    class Program
    {
        /// <summary>
        /// The pipe name the FileMonitor RPC service communicates over between processes.
        /// </summary>
        private const string CoreHookPipeName = "UwpCoreHook";

        /// <summary>
        /// The directory containing the CoreHook modules to be loaded in the target process.
        /// </summary>
        private const string HookLibraryDirName = "Hook";

        /// <summary>
        /// The library to be injected into the target process and executed
        /// using it's 'Run' Method.
        /// </summary>
        private const string HookLibraryName = "CoreHook.Uwp.FileMonitor.Hook.dll";

        /// <summary>
        /// The name of the pipe used for notifying the host process
        /// if the hooking plugin has been loaded successfully in
        /// the target process or if loading failed.
        /// </summary>
        private const string InjectionPipeName = "UwpCoreHookInjection";

        /// <summary>
        /// Enable verbose logging to the console for the CoreCLR native host module.
        /// </summary>
        private const bool HostVerboseLog = false;

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

            string injectionLibrary = Path.Combine(currentDir, HookLibraryDirName, HookLibraryName);

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
            InjectDllIntoTarget(targetProcessId, injectionLibrary);

            // Start the RPC server for handling requests from the hooked app.
            StartListener();
        }
        /// <summary>
        /// Inject and load the CoreHook hooking module <paramref name="injectionLibrary"/>
        /// in the existing created process referenced by <paramref name="processId"/>.
        /// </summary>
        /// <param name="processId">The target process ID to inject and load plugin into.</param>
        /// <param name="injectionLibrary">The path of the plugin that is loaded into the target process.</param>
        /// <param name="injectionPipeName">The pipe name which receives messages during the plugin initialization stage.</param>
        private static void InjectDllIntoTarget(
            int processId, 
            string injectionLibrary,
            string injectionPipeName = InjectionPipeName)
        {
            if (Examples.Common.ModulesPathHelper.GetCoreLoadPaths(
                    ProcessHelper.GetProcessById(processId).Is64Bit(),
                    out NativeModulesConfiguration nativeConfig) &&
                Examples.Common.ModulesPathHelper.GetCoreLoadModulePath(
                    out string coreLoadLibrary))
            {
                // Make sure the native dll modules can be accessed by the UWP application
                GrantAllAppPackagesAccessToFile(nativeConfig.HostLibrary);
                GrantAllAppPackagesAccessToFile(nativeConfig.DetourLibrary);

                RemoteInjector.Inject(
                    processId,
                    new RemoteInjectorConfiguration(nativeConfig)
                    {
                        InjectionPipeName = injectionPipeName,
                        ClrBootstrapLibrary = coreLoadLibrary,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = HostVerboseLog
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
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".json") || name.EndsWith(".dll") || name.EndsWith(".pdb")))
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

            foreach (var filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".pdb")))
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
            while((path = Path.GetDirectoryName(path)) != rootDirectory)
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

                var rule = new FileSystemAccessRule(AllAppPackagesSid,
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
}
