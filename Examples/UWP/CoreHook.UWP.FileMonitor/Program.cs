using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using System.Reflection;
using CoreHook.UWP.FileMonitor.Pipe;
using CoreHook.FileMonitor.Service;
using System.IO;
using System.Diagnostics;
using System.Threading;
using CoreHook.FileMonitor.Service.Pipe;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CoreHook.UWP.FileMonitor
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

        const string MSPaintAppName = "Microsoft.MSPaint_8wekyb3d8bbwe!Microsoft.MSPaint";

        static void Main(string[] args)
        {
            int TargetPID = 0;
#if DEBUG
            string targetApp = MSPaintAppName;
#else
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
                    Console.WriteLine("   or: FileMonitor AppUserModelId");
                    Console.WriteLine();
                    Console.Write("Please enter a process Id or the App Id to launch: ");

                    args = new string[] { Console.ReadLine() };

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetApp = args[0];
                    break;
                }
            }
#endif
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string injectionLibrary = Path.Combine(currentDir,
                "netstandard2.0", "CoreHook.UWP.FileMonitor.Hook.dll");

            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMon injection dll");
                return;
            }

            string easyHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.Is64BitProcess ? "EasyHook64.dll" : "EasyHook32.dll");

            GrantAllAppPkgsAccessToDir(currentDir);
            GrantAllAppPkgsAccessToDir(Path.Combine(currentDir, "netstandard2.0"));

            // start process and begin dll loading
            if (!string.IsNullOrEmpty(targetApp))
            {
                TargetPID = LaunchAppxPackageForPid(targetApp);
            }

            // inject FileMon dll into process
            InjectDllIntoTarget(TargetPID, injectionLibrary, easyHookDll);

            // start RPC server
            StartListener();

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
            var coreRunDll = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "CoreRunDLL64.dll" : "CoreRunDLL32.dll");
            if (!File.Exists(easyHookDll))
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

        private static void GrantAllAppPkgsAccessToDir(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            GrantAllAppPkgsAccessToFile(directory);
            foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".json") || name.EndsWith(".dll")))
            {
                GrantAllAppPkgsAccessToFile(file);
            }
        }
        private static void GrantAllAppPkgsAccessToFile(string fileName)
        {
            try
            {
                var acl = File.GetAccessControl(fileName);

                var rule = new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                acl.SetAccessRule(rule);

                File.SetAccessControl(fileName, acl);
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

            // PackageFamilyName + {Applications.Application.Id}, inside AppxManifest.xml
            appActiveManager.ActivateApplication(appName, null, ActivateOptions.None, out pid);

            return (int)pid;
        }
    }
}
