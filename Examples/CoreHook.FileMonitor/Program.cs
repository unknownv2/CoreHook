using System;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using System.Reflection;
using CoreHook.FileMonitor.Pipe;
using CoreHook.FileMonitor.Service;
using System.IO;
using System.Diagnostics;
using CoreHook.FileMonitor.Service.Pipe;
using System.Runtime.InteropServices;

namespace CoreHook.FileMonitor
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

        static void Main(string[] args)
        {
            int TargetPID = 0;
            string targetProgam = string.Empty;
            // Load the parameter
            while ((args.Length != 1) || !Int32.TryParse(args[0], out TargetPID) || !File.Exists(args[0]))
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

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetProgam = args[0];
                    break;
                }
            }

            string injectionLibrary = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "netstandard2.0", "CoreHook.FileMonitor.Hook.dll");
            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMonitor injection dll");
                return;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string easyHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Environment.Is64BitProcess ? "EasyHook64.dll" : "EasyHook32.dll");
                if (!File.Exists(easyHookDll))
                {
                    Console.WriteLine("Cannot find EasyHook dll");
                    return;
                }
                // start process and begin dll loading
                if (!string.IsNullOrEmpty(targetProgam))
                {
                    TargetPID = Process.Start(targetProgam).Id;
                }

                // inject FileMonitor dll into process
                InjectDllIntoTarget(TargetPID, injectionLibrary, easyHookDll);
            }
            else
            {
                throw new Exception("Unsupported platform detected");
            }
            // start RPC server
            StartListener();
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
        static void InjectDllIntoTarget(int procId, string injectionLibrary, string easyHookDll)
        {
            if (!File.Exists(easyHookDll))
            {
                Console.WriteLine("Cannot find EasyHook dll");
                return;
            }

            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            var coreLibrariesPath = Environment.GetEnvironmentVariable("CORE_LIBRARIES");
            var coreRootPath = Environment.GetEnvironmentVariable("CORE_ROOT");

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // path to CoreRunDLL.dll
            var coreRunDll = Path.Combine(currentDir, "CoreRunDLL.dll");
            if (!File.Exists(coreRunDll))
            {
                coreRunDll = Environment.GetEnvironmentVariable("CORERUNDLL");
                if (!File.Exists(coreRunDll))
                {
                    Console.WriteLine("Cannot find CoreRun dll");
                    return;
                }
            }
            // path to CoreHook.CoreLoad.dll
            var coreLoadDll = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadDll))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return;
            }

            // for now, we use the EasyHook dll to support function hooking on Windows
            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                easyHookDll);

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunDll,
                coreLoadDll,
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                CoreHookPipeName);
        }

        static void StartListener()
        {
            var _listener = new NpListener(CoreHookPipeName);
            _listener.RequestRetrieved += ClientConnectionMade;
            _listener.Start();

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }

        static void ClientConnectionMade(object sender, PipeClientConnectionEventArgs args)
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
