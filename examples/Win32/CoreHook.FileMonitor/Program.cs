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
        private const string CoreHookPipeName = "CoreHook";
        private const string HookLibraryDirName = "Hook";
        private const string HookLibraryName = "CoreHook.FileMonitor.Hook.dll";

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

        private static IPipePlatform pipePlatform = new PipePlatform();

        /// <summary>
        /// Parse a file path and remove quotes from path name if it is enclosed
        /// </summary>
        /// <param name="filePath">A  path to a file or directory.</param>
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

            int targetPID = 0;// System.Diagnostics.Process.GetProcessesByName("TestCPP")[0].Id;
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

            // start process and begin dll loading
            if (!string.IsNullOrEmpty(targetProgam))
            {
                CreateAndInjectDll(targetProgam, injectionLibrary);
            }
            else
            {
                // inject FileMonitor dll into process
                InjectDllIntoTarget(targetPID, injectionLibrary);
            }
            
            // start RPC server
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

        private static void CreateAndInjectDll(string exePath, string injectionLibrary)
        {
            ValidateFilePath(exePath);
            ValidateFilePath(injectionLibrary);

            CoreHookNativeConfig configX86;
            CoreHookNativeConfig configX64;
            if (Examples.Common.Utilities.GetCoreLoadPaths(false, out configX86)
                && Examples.Common.Utilities.GetCoreLoadPaths(true, out configX64))
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
                         PayloadLibrary = injectionLibrary,
                         VerboseLog = HostVerboseLog,
                         WaitForDebugger = HostWaitForDebugger,
                         StartAssembly = HostStartAssembly
                     },
                     pipePlatform,
                     out _,
                     CoreHookPipeName);
            }
        }

        private static void InjectDllIntoTarget(int procId, string injectionLibrary)
        {
            ValidateFilePath(injectionLibrary);

            string coreRunDll, coreLibrariesPath, coreRootPath, coreLoadDll, coreHookDll;
            if (Examples.Common.Utilities.GetCoreLoadPaths(ProcessHelper.GetProcessById(procId).Is64Bit(),
                out coreRunDll, out coreLibrariesPath, out coreRootPath, out coreLoadDll, out coreHookDll))
            {
                RemoteHooking.Inject(
                    procId,
                    new RemoteHookingConfig()
                    {
                        HostLibrary = coreRunDll,
                        CoreCLRPath = coreRootPath,
                        CoreCLRLibrariesPath = coreLibrariesPath,
                        CLRBootstrapLibrary = coreLoadDll,
                        DetourLibrary = coreHookDll,
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
    }
}