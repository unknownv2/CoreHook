using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHook.FileMonitor.Service;
using CoreHook.ManagedHook.Remote;
using CoreHook.ManagedHook.ProcessUtils;
using CoreHook.FileMonitor.Service.Pipe;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;

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

        static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new UnsupportedPlatformException("Win32 example");
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

                    args = new string[] { Console.ReadLine() };

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

            string injectionLibrary = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Hook", "CoreHook.FileMonitor.Hook.dll");
            
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
            
            // start RPC server
            StartListener();
        }

        // info on these environment variables: 
        // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
        private static string GetCoreLibrariesPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                 Environment.Is64BitProcess ?
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_64") :
                 Environment.GetEnvironmentVariable("CORE_LIBRARIES_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string GetCoreRootPath()
        {
            return !ProcessHelper.IsArchitectureArm() ?
             (
                Environment.Is64BitProcess ?
                Environment.GetEnvironmentVariable("CORE_ROOT_64") :
                Environment.GetEnvironmentVariable("CORE_ROOT_32")
             )
             : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Retrieve the required paths for initializing the CoreCLR and executing .NET assemblies in an unmanaged process
        /// </summary>
        /// <param name="coreRunPath">The native module that we call to execute host and execute our hooking dll in the target process</param>
        /// <param name="coreLibsPath">Path to the CoreCLR dlls that implement the .NET Core runtime</param>
        /// <param name="coreRootPath">Path to the CoreCLR dlls  that implement the .NET Core runtime</param>
        /// <param name="coreLoadPath">Initial .NET module that loads and executes our hooking dll, and handles dependency resolution.</param>
        /// <returns>Returns wether all required paths and modules have been found.</returns>
        private static bool GetCoreLoadPaths(out string coreRunPath, out string coreLibsPath, out string coreRootPath, out string coreLoadPath)
        {
            coreRunPath = string.Empty;
            coreLoadPath = string.Empty;

            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Paths to the CoreCLR runtime dlls used to host and execute .NET assemblies 
            coreLibsPath = GetCoreLibrariesPath();
            coreRootPath = GetCoreRootPath();

            if (string.IsNullOrEmpty(coreLibsPath))
            {
                Console.WriteLine("CORE_LIBRARIES path was not set!");
                return false;
            }
            if (string.IsNullOrEmpty(coreRootPath))
            {
                Console.WriteLine("CORE_ROOT path was not set!");
                return false;
            }

            // Module  that initializes the .NET Core runtime and executes .NET assemblies
            coreRunPath = Path.Combine(currentDir,
                Environment.Is64BitProcess ? "corerundll64.dll" : "corerundll32.dll");
            if (!File.Exists(coreRunPath))
            {
                coreRunPath = Environment.GetEnvironmentVariable("CORERUNDLL");
                if (!File.Exists(coreRunPath))
                {
                    Console.WriteLine("Cannot find CoreRun dll");
                    return false;
                }
            }

            // Module that loads and executes the IEntryPoint.Run method of our hook dll.
            // It also resolves any dependencies for the hook dll
            coreLoadPath = Path.Combine(currentDir, "CoreHook.CoreLoad.dll");

            if (!File.Exists(coreLoadPath))
            {
                Console.WriteLine("Cannot find CoreLoad dll");
                return false;
            }

            return true;
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
                throw new ArgumentNullException($"File path {filePath} does not exist");
            }
        }
        private static void CreateAndInjectDll(string exePath, string injectionLibrary, string coreHookDll)
        {
            ValidateFilePath(exePath);
            ValidateFilePath(injectionLibrary);
            ValidateFilePath(coreHookDll);

            string coreRunDll, coreLibrariesPath, coreRootPath, coreLoadDll;
            if (GetCoreLoadPaths(out coreRunDll, out coreLibrariesPath, out coreRootPath, out coreLoadDll))
            {
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
                    out _,
                    new PipePlatform(),
                    null,
                    CoreHookPipeName);
            }
                   
        }
        private static void InjectDllIntoTarget(int procId, string injectionLibrary, string coreHookDll)
        {
            ValidateFilePath(injectionLibrary);
            ValidateFilePath(coreHookDll);

            string coreRunDll, coreLibrariesPath, coreRootPath, coreLoadDll;
            if (GetCoreLoadPaths(out coreRunDll, out coreLibrariesPath, out coreRootPath, out coreLoadDll))
            {
                RemoteHooking.Inject(
                    procId,
                    coreRunDll,
                    coreLoadDll,
                    coreRootPath, // path to coreclr, clrjit
                    coreLibrariesPath, // path to .net core shared libs
                    injectionLibrary,
                    injectionLibrary,
                    new PipePlatform(),
                    new[] { coreHookDll },
                    CoreHookPipeName);
            }        
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
    }
}