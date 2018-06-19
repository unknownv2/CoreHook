using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using JsonRpc.DynamicProxy.Client;
using JsonRpc.Standard.Client;
using JsonRpc.Standard.Contracts;
using JsonRpc.Streams;
using CoreHook.IPC.Pipes.Client;

namespace CoreHook.Unix.FileMonitor.Hook
{
    public class Library : IEntryPoint
    {
        private static readonly IJsonRpcContractResolver myContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        Stack<String> Queue = new Stack<String>();

        LocalHook OpenHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Ansi,
            SetLastError = true)]
        delegate Int32 DOpen(
            String pathname, int flags, int mode);

        public Library(object InContext, string arg1)
        {
        }

        public void Run(object InContext, string pipeName)
        {
            try
            {
                StartClient(pipeName);
            }
            catch (Exception ex)
            {
                ClientWriteLine(ex.ToString());
            }
        }
        private static void ClientWriteLine(object msg)
        {
            Console.WriteLine(msg);
        }

        public void StartClient(string pipeName)
        {
            var clientPipe = new ClientPipe(pipeName);

            var clientTask = RunClientAsync(clientPipe.Start());

            // Wait for the client to exit.
            clientTask.GetAwaiter().GetResult();
        }

        private void CreateHooks()
        {
            ClientWriteLine("Adding hook to 'open' function");

            ImportUtils.ILibLoader dllLoadUtils = null;
            IntPtr dllHandle = IntPtr.Zero;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                dllLoadUtils = new ImportUtils.LibLoaderUnix();
                dllHandle = dllLoadUtils.LoadLibrary("/lib/x86_64-linux-gnu/libc.so.6");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dllLoadUtils = new ImportUtils.LibLoaderMacOS();
                dllHandle = dllLoadUtils.LoadLibrary("/usr/lib/libSystem.dylib");
            }

            var functionHandle = dllLoadUtils.GetProcAddress(dllHandle, "open");

            Console.WriteLine($"'open' function is at {functionHandle.ToInt64().ToString("X")}");

            ClientWriteLine("Creating 'open' hook");
     
            OpenHook = LocalHook.Create(
                functionHandle,
                new DOpen(open_hook),
                this);

            OpenHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
        }
        const string LIBC = "libc";
        [DllImport(LIBC, SetLastError = true)]
        internal static extern int open(String pathname, int flags, int mode);

        static int open_hook(String pathname, int flags, int mode)
        {
            Console.WriteLine($"Opening {pathname}...");
            Library This = (Library)HookRuntimeInfo.Callback;

            lock (This.Queue)
            {
                This.Queue.Push(pathname);
            }
            return open(pathname, flags, mode);
        }

        private async Task RunClientAsync(Stream clientStream)
        {
            await Task.Yield(); // We want this task to run on another thread.
            var clientHandler = new StreamRpcClientHandler();

            using (var reader = new ByLineTextMessageReader(clientStream))
            using (var writer = new ByLineTextMessageWriter(clientStream))
            using (clientHandler.Attach(reader, writer))
            {

                var client = new JsonRpcClient(clientHandler);

                var builder = new JsonRpcProxyBuilder
                {
                    ContractResolver = myContractResolver
                };

                var proxy = builder.CreateProxy<CoreHook.FileMonitor.Shared.IFileMonitor>(client);

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
                            await proxy.OnCreateFile(Package);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ClientWriteLine(ex.ToString());
                }
            }
        }
    }
    interface DllLoadUtils
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
    }
    internal class DllLoadUtilsLinux : DllLoadUtils
    {
        public IntPtr LoadLibrary(string fileName)
        {
            return dlopen(fileName, RTLD_NOW);
        }

        public void FreeLibrary(IntPtr handle)
        {
            dlclose(handle);
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string name)
        {
            // clear previous errors if any
            dlerror();
            var res = dlsym(dllHandle, name);
            var errPtr = dlerror();
            if (errPtr != IntPtr.Zero)
            {
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
            }
            return res;
        }

        const int RTLD_NOW = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, String symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();
    }
}
