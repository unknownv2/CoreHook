using System;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHook.FileMonitor.Service;
using CoreHook.FileMonitor.Service.Pipe;
using CoreHook.ManagedHook.Remote;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;

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

        private const string CoreHookPipeName = "UWPCoreHook";
        private static IPC.Platform.IPipePlatform pipePlatform = new Pipe.PipePlatform();

        private static bool IsArchitectureArm()
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }

        private static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new UnsupportedPlatformException("UWP example");
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

            string injectionLibrary = Path.Combine(currentDir,
                "Hook", "CoreHook.UWP.FileMonitor.Hook.dll");

            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMonitor injection dll");
                return;
            }

            string coreHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.Is64BitProcess ? "corehook64.dll" : "corehook32.dll");

            GrantAllAppPkgsAccessToDir(currentDir);
            GrantAllAppPkgsAccessToDir(Path.GetDirectoryName(injectionLibrary));

            // Start the target process and begin dll loading
            if (!string.IsNullOrEmpty(targetApp))
            {
                targetPID = LaunchAppxPackageForPid(targetApp);
            }

            // Inject the FileMonitor.Hook dll into the process
            InjectDllIntoTarget(targetPID, injectionLibrary, coreHookDll);

            // Start the RPC server for handling requests from the hooked app
            StartListener();
        }

        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        private static string GetCoreLibrariesPath()
        {
            return !IsArchitectureArm() ?
             Environment.Is64BitProcess? 
             Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
             Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string GetCoreRootPath()
        {
            return !IsArchitectureArm() ?
             Environment.Is64BitProcess ?
             Environment.GetEnvironmentVariable("CORE_ROOT_64") :
             Environment.GetEnvironmentVariable("CORE_ROOT_32")
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static void InjectDllIntoTarget(int procId, string injectionLibrary, string coreHookDll)
        {
            if (!File.Exists(coreHookDll))
            {
                Console.WriteLine("Cannot find corehook dll");
                return;
            }

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string coreLibrariesPath = GetCoreLibrariesPath();
            string coreRootPath = GetCoreRootPath();
            if (string.IsNullOrEmpty(coreLibrariesPath))
            {
                Console.WriteLine("CORE_LIBRARIES path was not set!");
                return;
            }
            if (string.IsNullOrEmpty(coreRootPath))
            {
                Console.WriteLine("CORE_ROOT path was not set!");
                return;
            }
            // path to CoreRunDLL.dll
            string coreRunDll = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "corerundll64.dll" : "corerundll32.dll");
            if (!File.Exists(coreRunDll))
            {
                coreRunDll = Environment.GetEnvironmentVariable("CORERUNDLL");
                if (!File.Exists(coreRunDll))
                {
                    Console.WriteLine("Cannot find CoreRun dll");
                    return;
                }
                else
                {
                    GrantAllAppPkgsAccessToFile(coreRunDll);
                }
            }
            // path to CoreHook.CoreLoad.dll
            string coreLoadDll = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

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
                pipePlatform,
                new [] { coreHookDll },
                CoreHookPipeName);
        }

        private static void StartListener()
        {
            var listener = new NpListener(CoreHookPipeName, pipePlatform);
            listener.RequestRetrieved += ClientConnectionMade;
            listener.Start();

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }

        private static void ClientConnectionMade(object sender, PipeClientConnectionEventArgs args)
        {
            var pipeServer = args.PipeStream;

            IJsonRpcServiceHost host = BuildServiceHost();

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
            {
                return;
            }

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
                var fInfo = new FileInfo(fileName);
                FileSecurity acl = fInfo.GetAccessControl();

                var rule = new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                acl.SetAccessRule(rule);

                fInfo.SetAccessControl(acl);
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
