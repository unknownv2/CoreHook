using System;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using System.Reflection;
using CoreHook.FileMonitor.Pipe;
using CoreHook.FileMonitor.Service;
using CoreHook.ManagedHook.Remote;
using System.IO;
using CoreHook.ManagedHook.ProcessUtils;
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

        private const string CoreHookPipeName = "CoreHook";
        private static IPC.Platform.IPipePlatform pipePlatform = new PipePlatform();

        static void Main(string[] args)
        {
            int targetPID = 0;
            string targetProgam = string.Empty;
            // Load the parameter
            while ((args.Length != 1) || !Int32.TryParse(args[0], out targetPID) || !File.Exists(args[0]))
            {
                if (targetPID > 0)
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
                string coreHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Environment.Is64BitProcess ? "corehook64.dll" : "corehook32.dll");

                // start process and begin dll loading
                if (!string.IsNullOrEmpty(targetProgam))
                {
                    CreateAndInjectDll(targetProgam, injectionLibrary, coreHookDll);
                }
                else
                {
                    // inject FileMonitor dll into process
                    InjectDllIntoTarget(targetPID, injectionLibrary, coreHookDll);
                }
            }
            else
            {
                throw new Exception("Unsupported platform detected");
            }

            // start RPC server
            StartListener();
        }

        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        private static string GetCoreLibrariesPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             Environment.Is64BitProcess ?
             Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
             Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string GetCoreRootPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             Environment.Is64BitProcess ?
             Environment.GetEnvironmentVariable("CORE_ROOT_64") :
             Environment.GetEnvironmentVariable("CORE_ROOT_32")
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static void CreateAndInjectDll(string exePath, string injectionLibrary, string coreHookDll)
        {
            if (!File.Exists(coreHookDll))
            {
                Console.WriteLine("Cannot find corehook dll");
                return;
            }
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var coreLibrariesPath = GetCoreLibrariesPath();
            var coreRootPath = GetCoreRootPath();

            // path to CoreRunDLL.dll
            var coreRunDll = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "CoreRunDLL64.dll" : "CoreRunDLL32.dll");
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

            int processId;
            RemoteHooking.CreateAndInject(
                exePath,
                coreHookDll,
                coreRunDll,
                coreLoadDll,
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                null,
                0,
                injectionLibrary,
                injectionLibrary,
                out processId,
                new PipePlatform(),
                null,
                CoreHookPipeName);
        }
        private static void InjectDllIntoTarget(int procId, string injectionLibrary, string coreHookDll)
        {
            if (!File.Exists(coreHookDll))
            {
                Console.WriteLine("Cannot find corehook dll");
                return;
            }
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            var coreLibrariesPath = GetCoreLibrariesPath();
            var coreRootPath = GetCoreRootPath();

            // path to CoreRunDLL.dll
            var coreRunDll = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "CoreRunDLL64.dll" : "CoreRunDLL32.dll");
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
            
            RemoteHooking.Inject(
                procId,
                coreRunDll,
                coreLoadDll,
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                new PipePlatform(),
                new []{ coreHookDll },
                CoreHookPipeName);
        }

        private static void StartListener()
        {
            var _listener = new NpListener(CoreHookPipeName, pipePlatform);
            _listener.RequestRetrieved += ClientConnectionMade;
            _listener.Start();

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