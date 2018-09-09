using System;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using System.Reflection;
using CoreHook.Unix.FileMonitor.Pipe;
using CoreHook.FileMonitor.Service;
using System.IO;
using System.Diagnostics;
using CoreHook.FileMonitor.Service.Pipe;
using System.Runtime.InteropServices;

namespace CoreHook.Unix.FileMonitor
{
    class Program
    {
        private static readonly IJsonRpcContractResolver myContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        const string CoreHookPipeName = "CoreHook";
        static bool IsArchitectureArm()
        {
            var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }
        static void Main(string[] args)
        {
            int TargetPID = 0;
            string targetProgam = string.Empty;
            IsArchitectureArm();
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

            string injectionLibrary = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Hook", "CoreHook.Unix.FileMonitor.Hook.dll");
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

            // start RPC server
            StartListener();
        }
        static void MacOSInjectDllIntoTarget(int procId, string injectionLibrary)
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var coreLibrariesPath = "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.1.0";

            // path to CoreHook.CoreLoad.dll
            var coreLoadDll = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadDll))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return;
            }
            var coreRunLib = Path.Combine(currentDir, "libcorerun.dylib");
            if (!File.Exists(coreRunLib))
            {
                Console.WriteLine("Cannot find CoreRun library");
                return;
            }

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunLib,
                coreLoadDll,
                coreLibrariesPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                new PipePlatform(),
                null,
                CoreHookPipeName);
        }
        static void LinuxInjectDllIntoTarget(int procId, string injectionLibrary)
        {
            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            //var coreLibrariesPath = Environment.GetEnvironmentVariable("CORE_LIBRARIES");
            //var coreRootPath = Environment.GetEnvironmentVariable("CORE_ROOT");
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var coreLibrariesPath = "/usr/share/dotnet/shared/Microsoft.NETCore.App/2.1.0/";

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

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunLib,
                coreLoadDll,
                coreLibrariesPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                new PipePlatform(),
                null,
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
            var listener = new NpListener(CoreHookPipeName);
            listener.RequestRetrieved += ClientConnectionMade;
            listener.Start();

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }

        private static void ClientConnectionMade(object sender, PipeClientConnectionEventArgs args)
        {
            var pipeServer = args.PipeStream;

            var host = BuildServiceHost();

            var serverHandler = new StreamRpcServerHandler(host);

            var session = new FileMonitorSessionFeature();
            serverHandler.DefaultFeatures.Set(session);

            using (var reader = new ByLineTextMessageReader(pipeServer))
            using (var writer = new ByLineTextMessageWriter(pipeServer))
            using (serverHandler.Attach(reader, writer))
            {
                // Wait for exit
                session.CancellationToken.WaitHandle.WaitOne();
            }
        }
        private static IJsonRpcServiceHost BuildServiceHost()
        {
            var builder = new JsonRpcServiceHostBuilder
            {
                ContractResolver = myContractResolver,
            };

            // Register FileMonitorService for RPC
            builder.Register(typeof(FileMonitorService));

            builder.Intercept(async (context, next) =>
            {
                Console.WriteLine("> {0}", context.Request);
                await next();
                Console.WriteLine("< {0}", context.Response);
            });
            return builder.Build();
        }
    }
}
