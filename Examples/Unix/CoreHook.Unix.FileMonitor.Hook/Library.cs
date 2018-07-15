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
                dllHandle = dllLoadUtils.LoadLibrary("libc.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dllLoadUtils = new ImportUtils.LibLoaderMacOS();
                dllHandle = dllLoadUtils.LoadLibrary("/usr/lib/libc.dylib");
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
}
