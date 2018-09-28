using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHook.FileMonitor.Service;
using CoreHook.IPC.Platform;
using CoreHook.ManagedHook.Remote;

namespace CoreHook.Unix.FileMonitor
{
    class Program
    {
        private const string CoreHookPipeName = "CoreHook";
        private const string HookLibraryDirName = "Hook";
        private const string HookLibraryName = "CoreHook.Unix.FileMonitor.Hook.dll";

        private const string CoreLibrariesPathOSX = "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.1.0";
        private const string CoreLibrariesPathLinux = "/usr/share/dotnet/shared/Microsoft.NETCore.App/2.1.0/";

        private static IPipePlatform pipePlatform = new PipePlatform();

        private static bool IsArchitectureArm()
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }

        private static void Main(string[] args)
        {
            int TargetPID = 0;
            string targetProgam = string.Empty;

            // Load the parameter
            while ((args.Length != 1) || !int.TryParse(args[0], out TargetPID) || !File.Exists(args[0]))
            {
                if (TargetPID > 0)
                {
                    break;
                }
                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage: FileMonitor %PID%");
                    Console.WriteLine("   or: FileMonitor PathToExecutable");
                    Console.WriteLine();
                    Console.Write("Please enter a process Id or path to executable: ");

                    args = new string[] { Console.ReadLine() };

                    if (string.IsNullOrEmpty(args[0]))
                    {
                        return;
                    }
                }
                else
                {
                    targetProgam = args[0];
                    break;
                }
            }

            string injectionLibrary = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                HookLibraryDirName, HookLibraryName);

            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMonitor injection dll");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LinuxInjectDllIntoTarget(TargetPID, injectionLibrary);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                MacOSInjectDllIntoTarget(TargetPID, injectionLibrary);
            }
            else
            {
                throw new UnsupportedPlatformException("Unix FileMonitor example");
            }

            // start RPC server
            StartListener();
        }

        static void MacOSInjectDllIntoTarget(int procId, string injectionLibrary)
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var coreLibrariesPath = CoreLibrariesPathOSX;

            var coreLoadDll = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadDll))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return;
            }
            var coreRunLib = Path.Combine(currentDir, "libcorerun.dylib");
            if (!File.Exists(coreRunLib))
            {
                Console.WriteLine("Cannot find corerun library");
                return;
            }

            RemoteHooking.Inject(
                procId,
                new RemoteHookingConfig()
                {
                    HostLibrary = coreRunLib,
                    CoreCLRPath = coreLibrariesPath,
                    CoreCLRLibrariesPath = coreLibrariesPath,
                    CLRBootstrapLibrary = coreLoadDll,
                    DetourLibrary = null,
                    PayloadLibrary = injectionLibrary,
                    VerboseLog = false,
                    WaitForDebugger = false,
                    StartAssembly = false
                },
                pipePlatform,
                CoreHookPipeName);
        }

        static void LinuxInjectDllIntoTarget(int procId, string injectionLibrary)
        {
            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            //var coreLibrariesPath = Environment.GetEnvironmentVariable("CORE_LIBRARIES");
            //var coreRootPath = Environment.GetEnvironmentVariable("CORE_ROOT");
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var coreLibrariesPath = CoreLibrariesPathLinux;

            // path to CoreHook.CoreLoad.dll
            var coreLoadDll = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadDll))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return;
            }
            var coreRunLib = Path.Combine(currentDir, "libcorerun.so");
            if (!File.Exists(coreRunLib))
            {
                Console.WriteLine("Cannot find CoreRun library");
                return;
            }

            RemoteHooking.Inject(
                procId,
                new RemoteHookingConfig()
                {
                    HostLibrary = coreRunLib,
                    CoreCLRPath = coreLibrariesPath,
                    CoreCLRLibrariesPath = coreLibrariesPath,
                    CLRBootstrapLibrary = coreLoadDll,
                    DetourLibrary = string.Empty,
                    PayloadLibrary = injectionLibrary,
                    VerboseLog = false,
                    WaitForDebugger = false,
                    StartAssembly = false
                },
                pipePlatform,
                CoreHookPipeName);
        }
        private static Process[] GetProcessListByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        private static Process GetProcessById(int processId)
        {
            return Process.GetProcessById(processId);
        }

        private static Process GetProcessByName(string processName)
        {
            return GetProcessListByName(processName)[0];
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
