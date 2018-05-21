using System;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using System.Reflection;
using CoreHook.FileMonitor.Pipe;
using CoreHook.FileMonitor.Service;
using System.IO;
using System.Diagnostics;

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
            string targetExe = null;
            
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
                    Console.WriteLine("Usage: FileMon %PID%");
                    Console.WriteLine("   or: FileMon PathToExecutable");
                    Console.WriteLine();
                    Console.Write("Please enter a process Id or path to executable: ");

                    args = new string[] { Console.ReadLine() };

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetExe = args[0];
                    break;
                }
            }            


            string injectionLibrary = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "netstandard2.0", "CoreHook.FileMonitor.Hook.dll");
            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMon injection dll");
                return;
            }

            string easyHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "EasyHook64.dll");
            if (!File.Exists(easyHookDll))
            {
                Console.WriteLine("Cannot find EasyHook dll");
                return;
            }
            // start process and begin dll loading
            if (!string.IsNullOrEmpty(targetExe))
            {
                TargetPID = Process.Start(targetExe).Id;
            }

            // inject FileMon dll into process
            InjectDllIntoTarget(TargetPID, injectionLibrary, easyHookDll);

            // start RPC server
            StartListener();

        }
        static void InjectDllIntoTarget(int procId, string injectionLibrary, string easyHookDll)
        {            
            // for now, we use the EasyHook dll to support function hooking on Windows
            ManagedHook.Remote.RemoteHooking.Inject(
                procId, 
                easyHookDll);

            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            var coreLibrariesPath = Environment.GetEnvironmentVariable("CORE_LIBRARIES");
            var coreRootPath = Environment.GetEnvironmentVariable("CORE_ROOT");
            // path to CoreRunDLL.dll
            var coreRunDll = Environment.GetEnvironmentVariable("CORERUNDLL");
            // path to CoreHook.CoreLoad.dll
            var coreLoadDll = Environment.GetEnvironmentVariable("CORE_LOAD");

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunDll, // CoreRun.dll
                coreLoadDll, // CoreLoad dll
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                CoreHookPipeName);
        }
     
        static void StartListener()
        {
            var _listener = new NpListener(CoreHookPipeName, log: null, stats: null);
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
