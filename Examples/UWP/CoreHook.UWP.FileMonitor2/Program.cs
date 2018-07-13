﻿using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using CoreHook.UWP.FileMonitor2.Pipe;
using CoreHook.FileMonitor.Service;
using System.Reflection;
using CoreHook.FileMonitor.Service.Pipe;

namespace CoreHook.UWP.FileMonitor2
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
        static PipeHelper pipeHelper = new PipeHelper(CoreHookPipeName);
        private static bool IsArchitectureArm()
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }
        private static void CreatePipeHelper()
        {
            pipeHelper.Start();
        }
        private static void WaitForHook()
        {
            while (!pipeHelper.Started)
            {
                Thread.Sleep(500);
            }
        }
        static void Main(string[] args)
        {   
            Task.Factory.StartNew(() => CreatePipeHelper(), TaskCreationOptions.LongRunning);
            WaitForHook();

            int TargetPID = 0;

            string targetApp = string.Empty;

            // Load the parameter
            while ((args.Length != 1) || !Int32.TryParse(args[0], out TargetPID))
            {
                if (TargetPID > 0)
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

                    args = new string[] { Console.ReadLine() };

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetApp = args[0];
                    break;
                }
            }

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string injectionLibrary = Path.Combine(currentDir,
                "../netstandard2.0", "CoreHook.UWP.FileMonitor.Hook.dll");

            if (!File.Exists(injectionLibrary))
            {
                Console.WriteLine("Cannot find FileMon injection dll");
                return;
            }

            string coreHookDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.Is64BitProcess ? "corehook64.dll" : "corehook32.dll");

            GrantAllAppPkgsAccessToDir(currentDir);
            GrantAllAppPkgsAccessToDir(Path.Combine(currentDir, "../netstandard2.0"));

            // start process and begin dll loading
            if (!string.IsNullOrEmpty(targetApp))
            {
                TargetPID = LaunchAppxPackageForPid(targetApp);
            }

            // inject FileMon dll into process
            InjectDllIntoTarget(TargetPID, injectionLibrary, coreHookDll);

            // start RPC server
            StartListener();
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
            var coreLibrariesPath = !IsArchitectureArm() ?
                Environment.GetEnvironmentVariable("CORE_LIBRARIES")
                : currentDir;
            var coreRootPath = !IsArchitectureArm() ?
                Environment.GetEnvironmentVariable("CORE_ROOT")
                : currentDir;

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

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunDll,
                coreLoadDll,
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                new PipePlatform(),
                new List<string>() { coreHookDll },
                CoreHookPipeName);
        }

        private static void StartListener()
        {
            var _listener = new NpListener(CoreHookPipeName);
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

        private static void GrantAllAppPkgsAccessToDir(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            //GrantAllAppPkgsAccessToFile(directory);
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
                var acl = fInfo.GetAccessControl();

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