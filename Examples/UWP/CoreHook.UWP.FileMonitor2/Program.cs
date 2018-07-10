using System;
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
    internal static partial class Libraries
    {
        internal const string Kernel32 = "kernel32.dll";
    }

    public class Library
    {
        private string PipeName;

        public Library(string pipeName)
        {
            PipeName = pipeName;
        }
        public void Start()
        {
            var clientTask = RunClientAsync();

            // Wait for the client to exit.
            clientTask.GetAwaiter().GetResult();
        }
        private async Task RunClientAsync()
        {
            await Task.Yield(); // We want this task to run on another thread.

            CreateHooks();

            try
            {
                while (true)
                {
                    Thread.Sleep(500);

                    if (Queue.Count > 0)
                    {
                        string[] Package = null;

                        lock (Queue)
                        {
                            Package = Queue.ToArray();

                            Queue.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClientWriteLine(ex.ToString());
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            internal uint nLength;
            internal IntPtr lpSecurityDescriptor;
            internal bool bInheritHandle;
        }

        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true,
            BestFitMapping = false, EntryPoint = "CreateNamedPipeW",
            CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CreateNamedPipe(
            string pipeName,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufferSize,
            int inBufferSize,
            int defaultTimeout,
            ref SECURITY_ATTRIBUTES securityAttributes);

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
             CharSet = CharSet.Unicode,
             SetLastError = true)]
        delegate IntPtr DCreateNamedPipe(
            string pipeName,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufferSize,
            int inBufferSize,
            int defaultTimeout,
            ref SECURITY_ATTRIBUTES securityAttributes);


        private static void ClientWriteLine(object msg)
        {
            Console.WriteLine(msg);
        }

        private static PipeSecurity CreateUWPPipeSecurity()
        {
            const PipeAccessRights access = PipeAccessRights.ReadWrite;

            var sec = new PipeSecurity();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                sec.AddAccessRule(
                    new PipeAccessRule(identity.User, access, AccessControlType.Allow)
                );

                if (identity.User != identity.Owner)
                {
                    sec.AddAccessRule(
                        new PipeAccessRule(identity.Owner, access, AccessControlType.Allow)
                    );
                }
            }

            // Allow all app packages to connect.
            sec.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), access, AccessControlType.Allow));
            return sec;
        }
        internal static unsafe SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, PipeSecurity pipeSecurity, ref GCHandle pinningHandle)
        {
            SECURITY_ATTRIBUTES secAttrs = default(SECURITY_ATTRIBUTES);
            secAttrs.nLength = (uint)sizeof(SECURITY_ATTRIBUTES);

            if ((inheritability & HandleInheritability.Inheritable) != 0)
            {
                secAttrs.bInheritHandle = true;
            }

            if (pipeSecurity != null)
            {
                byte[] securityDescriptor = pipeSecurity.GetSecurityDescriptorBinaryForm();
                pinningHandle = GCHandle.Alloc(securityDescriptor, GCHandleType.Pinned);
                fixed (byte* pSecurityDescriptor = securityDescriptor)
                {
                    secAttrs.lpSecurityDescriptor = (IntPtr)pSecurityDescriptor;
                }
            }

            return secAttrs;
        }
        Stack<String> Queue = new Stack<String>();
        LocalHook CreateNamedPipeHook;
        public bool Started = false;
        // this is where we are intercepting all file accesses!
        private static IntPtr CreateNamedPipe_Hooked(
            string pipeName,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufferSize,
            int inBufferSize,
            int defaultTimeout,
            ref SECURITY_ATTRIBUTES securityAttributes)

        {
            ClientWriteLine($"Creating pipe: '{pipeName}'...");

            try
            {
                Library This = (Library)HookRuntimeInfo.Callback;
                if (This != null)
                {
                    lock (This.Queue)
                    {
                        This.Queue.Push(pipeName);
                    }
                }

                if (pipeName.Contains(This.PipeName)) {
                    var pinningHandle = new GCHandle();
                    IntPtr result = IntPtr.Zero;
                    try
                    {
                        var security = GetSecAttrs(
                            HandleInheritability.None,
                            CreateUWPPipeSecurity(),
                            ref pinningHandle);

                        result = CreateNamedPipe(
                           pipeName,
                            openMode,
                            pipeMode,
                            maxInstances,
                            outBufferSize,
                            inBufferSize,
                            defaultTimeout,
                            ref security);
                    }
                    finally
                    {
                        if (pinningHandle.IsAllocated)
                        {
                            pinningHandle.Free();
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                ClientWriteLine(ex);
            }
            // call original API...
            return CreateNamedPipe(
               pipeName,
                openMode,
                pipeMode,
                maxInstances,
                outBufferSize,
                inBufferSize,
                defaultTimeout,
                ref securityAttributes);
        }
        private void CreateHooks()
        {
            ClientWriteLine("Adding hook to kernel32.dll!CreateNamedPipeW");

            CreateNamedPipeHook = LocalHook.Create(
                LocalHook.GetProcAddress(Libraries.Kernel32, "CreateNamedPipeW"),
                new DCreateNamedPipe(CreateNamedPipe_Hooked),
                this);

            CreateNamedPipeHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

            Started = true;
        }
    }
    /*
    class Program
    {
        static string PipeName = "CoreHook";
        static Library library = new Library(PipeName);
        static void Main(string[] args)
        {
            // hook named pipe

            Thread thread = new Thread(CreateLib);
            thread.Start();
            CreatePipe();

            Console.ReadLine();
        }
        private static void CreateLib()
        {
            library.Start();
        }
        private static void CreatePipe()
        {
            while(!library.Started)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("Hello World!");

            // create test pipe to trigger hook
            var testPipe = CreatePipe(PipeName);
        }
        private static NamedPipeServerStream CreatePipe(string pipeName)
        {
            return new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    254,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    65536,
                    65536
                    );
        }

    }*/
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
        static Library library = new Library(CoreHookPipeName);
        private static void CreateLib()
        {
            library.Start();
        }
        static void Main(string[] args)
        {
            Thread thread = new Thread(CreateLib);
            thread.Start();

            int TargetPID = 0;

            string targetApp = string.Empty;

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
                //TargetPID = LaunchAppxPackageForPid(targetApp);
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

            // info on these environment variables: 
            // https://github.com/dotnet/coreclr/blob/master/Documentation/workflow/UsingCoreRun.md
            var coreLibrariesPath = Environment.GetEnvironmentVariable("CORE_LIBRARIES");
            var coreRootPath = Environment.GetEnvironmentVariable("CORE_ROOT");

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
                coreHookDll);

            ManagedHook.Remote.RemoteHooking.Inject(
                procId,
                coreRunDll,
                coreLoadDll,
                coreRootPath, // path to coreclr, clrjit
                coreLibrariesPath, // path to .net core shared libs
                injectionLibrary,
                injectionLibrary,
                new PipePlatform(),
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
    }
}
