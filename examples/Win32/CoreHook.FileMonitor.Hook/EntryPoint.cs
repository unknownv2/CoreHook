using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using CoreHook.IPC.NamedPipes;
using JsonRpc.DynamicProxy.Client;
using JsonRpc.Standard.Client;
using JsonRpc.Standard.Contracts;
using JsonRpc.Streams;

namespace CoreHook.FileMonitor.Hook
{
    public class EntryPoint : IEntryPoint
    {
        private static readonly IJsonRpcContractResolver MyContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects.
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        private readonly Queue<string> _queue = new Queue<string>();

        private LocalHook _createFileHook;

        // The number of arguments in the constructor and Run method
        // must be equal to the number passed during injection
        // in the FileMonitor application.
        public EntryPoint(IContext context, string arg1) { }

        public void Run(IContext context, string pipeName)
        {
            try
            {
                StartClient(pipeName);
            }
            catch (Exception e)
            {
                ClientWriteLine(e.ToString());
            }
        }

        private static void ClientWriteLine(string msg) => Console.WriteLine(msg);

        private void StartClient(string pipeName)
        {
            var clientTask = RunClientAsync(NamedPipeClient.CreatePipeStream(pipeName));

            // Wait for the client to exit.
            clientTask.GetAwaiter().GetResult();
        }

        [UnmanagedFunctionPointer(
            CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        delegate IntPtr DelCreateFile(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        static extern IntPtr CreateFile(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);


        // Intercepts all file accesses and stores the requested filenames to a Queue.
        private static IntPtr CreateFile_Hooked(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            IntPtr securityAttributes,
            uint creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile)
        {

            ClientWriteLine($"Creating file: '{fileName}'...");

            try
            {
                EntryPoint This = (EntryPoint)HookRuntimeInfo.Callback;
                if (This != null)
                {
                    lock (This._queue)
                    {
                        This._queue.Enqueue(fileName);
                    }
                }
            }
            catch
            {

            }


            // Call original API function.
            return CreateFile(
                fileName,
                desiredAccess,
                shareMode,
                securityAttributes,
                creationDisposition,
                flagsAndAttributes,
                templateFile);
        }

        private void CreateHooks()
        {
            string[] functionName = { "kernel32.dll", "CreateFileW" };

            ClientWriteLine($"Adding hook to {functionName[0]}!{functionName[1]}");

            _createFileHook = LocalHook.Create(
                LocalHook.GetProcAddress(functionName[0], functionName[1]),
                new DelCreateFile(CreateFile_Hooked),
                this);

            _createFileHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
        }

        private async Task RunClientAsync(Stream clientStream)
        {
            await Task.Yield(); // We want this task to run on another thread.

            // Initialize the client connection to the RPC server.
            var clientHandler = new StreamRpcClientHandler();

            using (var reader = new ByLineTextMessageReader(clientStream))
            using (var writer = new ByLineTextMessageWriter(clientStream))
            using (clientHandler.Attach(reader, writer))
            {
                var builder = new JsonRpcProxyBuilder
                {
                    ContractResolver = MyContractResolver
                };

                var proxy = builder.CreateProxy<Shared.IFileMonitor>(new JsonRpcClient(clientHandler));

                // Create the function hooks after connection to the server.
                CreateHooks();

                try
                {
                    while (true)
                    {
                        Thread.Sleep(500);

                        if (_queue.Count > 0)
                        {
                            string[] package = null;

                            lock (_queue)
                            {
                                package = _queue.ToArray();

                                _queue.Clear();
                            }
                            await proxy.OnCreateFile(package);
                        }
                    }
                }
                catch (Exception e)
                {
                    ClientWriteLine(e.ToString());
                }
            }
        }
    }
}
