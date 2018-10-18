using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHook.FileMonitor.Service;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.ManagedHook.Remote;
using CoreHook.Unmanaged;

namespace CoreHook.FileMonitor
{
    class Program
    {
        /// <summary>
        /// The pipe name over which the FileMonitor RPC service communicates over between processes.
        /// </summary>
        private const string CoreHookPipeName = "CoreHook";
        /// <summary>
        /// The directory containing the CoreHook modules to be loaded in processes.
        /// </summary>
        private const string HookLibraryDirName = "Hook";
        /// <summary>
        /// The library injected to be injected the target processed and executed using it's 'Run' Method.
        /// </summary>
        private const string HookLibraryName = "CoreHook.FileMonitor.Hook.dll";

        /// <summary>
        /// Enable verbose logging to the console for the CoreCLR host module corerundll
        /// </summary>
        private const bool HostVerboseLog = true;
        /// <summary>
        /// Wait for a debugger to attach to the target process before running any .NET assemblies
        /// </summary>
        private const bool HostWaitForDebugger = false;
        /// <summary>
        /// Class that handles creating a named pipe server upong request
        /// </summary>
        private static IPipePlatform pipePlatform = new PipePlatform();

        /// <summary>
        /// Parse a file path and remove quotes from path name if it is enclosed
        /// </summary>
        /// <param name="filePath">A path to a file or directory.</param>
        /// <returns></returns>
        private static string GetFilePath(string filePath)
        {
            if(filePath == null)
            {
                throw new ArgumentNullException("Invalid file path name");
            }

            return filePath.Replace("\"", "");
        }

        private static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("Win32 example");
            }

            int targetPID = 0;
            string targetProgam = string.Empty;

            // Get the process to hook by file path for launching or process id for attaching
            while ((args.Length != 1) || !int.TryParse(args[0], out targetPID) || !File.Exists(GetFilePath(args[0])))
            {
                if (targetPID > 0)
                {
                    break;
                }
                if (args.Length != 1 || !File.Exists(GetFilePath(args[0])))
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
                    targetProgam = GetFilePath(args[0]);
                    break;
                }
            }

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string injectionLibrary = Path.Combine(currentDir, HookLibraryDirName, HookLibraryName);

            // Start process and begin dll loading
            if (!string.IsNullOrEmpty(targetProgam))
            {
                CreateAndInjectDll(targetProgam, injectionLibrary);
            }
            else
            {
                // Inject FileMonitor dll into process
                InjectDllIntoTarget(targetPID, injectionLibrary);
            }

            // Start the RPC server for handling requests from the hooked program
            StartListener();
        }
 
        /// <summary>
        /// Check if a file path is valid, otherwise throw an exception
        /// </summary>
        /// <param name="filePath">Path to a file or directory to validate</param>
        private static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException($"Invalid file path {filePath}");
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File path {filePath} does not exist");
            }
        }

        /// <summary>
        /// Start the application at <paramref name="exePath"/>
        /// and then inject and load the CoreHook hooking module <paramref name="injectionLibrary"/>
        /// in the newly created process.
        /// </summary>
        /// <param name="exePath">The path to the application to be launched.</param>
        /// <param name="injectionLibrary">The CoreHook hooking library to loaded in the target.</param>
        private static void CreateAndInjectDll(string exePath, string injectionLibrary)
        {
            ValidateFilePath(exePath);
            ValidateFilePath(injectionLibrary);

            if (Examples.Common.Utilities.GetCoreLoadPaths(
                false, out CoreHookNativeConfig configX86) &&
                Examples.Common.Utilities.GetCoreLoadPaths(
                true, out CoreHookNativeConfig configX64) &&
                Examples.Common.Utilities.GetCoreLoadModulePath(out string coreLoadLibrary))
            {
                RemoteHooking.CreateAndInject(
                     new ProcessCreationConfig()
                     {
                         ExecutablePath = exePath,
                         CommandLine = null,
                         ProcessCreationFlags = 0x00
                     },
                     configX86,
                     configX64,
                     new RemoteHookingConfig()
                     {
                         CLRBootstrapLibrary = coreLoadLibrary,
                         PayloadLibrary = injectionLibrary,
                         VerboseLog = HostVerboseLog,
                         WaitForDebugger = HostWaitForDebugger
                     },
                     pipePlatform,
                     out _,
                     CoreHookPipeName);
            }
        }

        /// <summary>
        /// Inject and load the CoreHook hooking module <paramref name="injectionLibrary"/>
        /// in the existing created process referenced by <paramref name="processId"/>.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="injectionLibrary"></param>
        private static void InjectDllIntoTarget(int processId, string injectionLibrary)
        {
            ValidateFilePath(injectionLibrary);

            if (Examples.Common.Utilities.GetCoreLoadPaths(
                ProcessHelper.GetProcessById(processId).Is64Bit(),
                out string coreRunDll, out string coreLibrariesPath,
                out string coreRootPath, out string coreLoadDll,
                out string coreHookDll))
            {
                RemoteHooking.Inject(
                    processId,
                    new RemoteHookingConfig()
                    {
                        HostLibrary = coreRunDll,
                        CoreCLRPath = coreRootPath,
                        CoreCLRLibrariesPath = coreLibrariesPath,
                        CLRBootstrapLibrary = coreLoadDll,
                        DetourLibrary = coreHookDll,
                        PayloadLibrary = injectionLibrary,
                        VerboseLog = HostVerboseLog,
                        WaitForDebugger = HostWaitForDebugger
                    },
                    pipePlatform,
                    CoreHookPipeName);
            }
        }

        /// <summary>
        /// Create an RPC server that is called by the RPC client started in
        /// a target process.
        /// </summary>
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
    }
}