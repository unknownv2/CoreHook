using System;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHook.FileMonitor.Service;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.Remote;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.Unmanaged;

namespace CoreHook.UWP.FileMonitor
{
    class Program
    {
        private const string CoreHookPipeName = "UWPCoreHook";
        private const string HookLibraryDirName = "Hook";
        private const string HookLibraryName = "CoreHook.UWP.FileMonitor.Hook.dll";

        /// <summary>
        /// Enable verbose logging to the console for the CoreCLR host module corerundll
        /// </summary>
        private const bool HostVerboseLog = false;
        /// <summary>
        /// Wait for a debugger to attach to the target process before running any .NET assemblies
        /// </summary>
        private const bool HostWaitForDebugger = false;
        /// <summary>
        /// Immediately start the .NET assembly if we are injecting a .NET Core application
        /// </summary>
        private const bool HostStartAssembly = false;

        private static IPipePlatform pipePlatform = new Pipe.PipePlatform();

        private static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("UWP example");
            }

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

                    if (string.IsNullOrEmpty(args[0]))
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

        private static void InjectDllIntoTarget(int procId, string injectionLibrary)
        {
            string coreRunDll, coreLibrariesPath, coreRootPath, coreLoadDll, corehookPath;
            if (Examples.Common.Utilities.GetCoreLoadPaths(ProcessHelper.GetProcessById(procId).Is64Bit(),
                out coreRunDll, out coreLibrariesPath, out coreRootPath, out coreLoadDll, out corehookPath))
            {
                // make sure the native dll modules can be accessed by the UWP application
                GrantAllAppPkgsAccessToFile(coreRunDll);
                GrantAllAppPkgsAccessToFile(corehookPath);

                RemoteHooking.Inject(
                    procId,
                    new RemoteHookingConfig()
                    {
                        HostLibrary = coreRunDll,
                        CoreCLRPath = coreRootPath,
                        CoreCLRLibrariesPath = coreLibrariesPath,
                        CLRBootstrapLibrary = coreLoadDll,
                        DetourLibrary = corehookPath,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = HostVerboseLog,
                        WaitForDebugger = HostWaitForDebugger,
                        StartAssembly = HostStartAssembly
                    },
                    pipePlatform,
                    CoreHookPipeName);
            }
        }

        private static void StartListener()
        {
            var session = new FileMonitorSessionFeature();

            Examples.Common.RpcService.CreateRpcService(
                  CoreHookPipeName,
                  pipePlatform,
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

        private static void GrantFolderRecursive(string fileName, string rootDir)
        {
            while((fileName = Path.GetDirectoryName(fileName)) != rootDir)
            {
                GrantAllAppPkgsAccessToFolder(fileName);
            }
        }

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
                return;
            }
        }

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
                return;
            }
        }

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
