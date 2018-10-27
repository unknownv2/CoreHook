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
using CoreHook.IPC.NamedPipes;

namespace CoreHook.Uwp.FileMonitor.Hook
{
    public class Library : IEntryPoint
    {
        private static readonly IJsonRpcContractResolver MyContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects.
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        Queue<string> Queue = new Queue<string>();

        LocalHook CreateFileHook;

        public Library(IContext context, string arg1) { }

        public void Run(IContext context, string pipeName)
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

        private static void ClientWriteLine(string msg) => Debug.WriteLine(msg);

        public void StartClient(string pipeName)
        {
            var clientTask = RunClientAsync(NamedPipeClient.CreatePipeStream(pipeName));

            // Wait for the client to exit.
            clientTask.GetAwaiter().GetResult();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        delegate IntPtr DCreateFile2(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            uint creationDisposition,
            IntPtr pCreateExParams);

        [DllImport("kernelbase.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true,
        CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFile2(
            string fileName,
            uint desiredAccess,
            uint shareMode,
            uint creationDisposition,
            IntPtr pCreateExParams);

        // this is where we are intercepting all file accesses!
        private static IntPtr CreateFile2_Hooked(
           string fileName,
           uint desiredAccess,
           uint shareMode,
           uint creationDisposition,
           IntPtr pCreateExParams)
        { 
            ClientWriteLine($"Creating file: '{fileName}'...");

            try
            {
                Library This = (Library)HookRuntimeInfo.Callback;
                if (This != null)
                {
                    lock (This.Queue)
                    {
                        This.Queue.Enqueue(fileName);
                    }
                }
            }
            catch
            {

            }

            // call original API...
            return CreateFile2(
                fileName,
                desiredAccess,
                shareMode,
                creationDisposition,
                pCreateExParams);
        }

        private void CreateHooks()
        {
            string[] functionName = new string[] { "kernelbase.dll", "CreateFile2" };
            ClientWriteLine($"Adding hook to {functionName[0]}!{functionName[1]}");

            CreateFileHook = LocalHook.Create(
                LocalHook.GetProcAddress(functionName[0], functionName[1]),
                new DCreateFile2(CreateFile2_Hooked),
                this);

            CreateFileHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
        }

        private async Task RunClientAsync(Stream clientStream)
        {
            await Task.Yield(); // We want this task to run on another thread.

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

                CreateHooks();

                try
                {
                    while (true)
                    {
                        Thread.Sleep(500);

                        if (Queue.Count > 0)
                        {
                            string[] package = null;

                            lock (Queue)
                            {
                                package = Queue.ToArray();

                                Queue.Clear();
                            }
                            await proxy.OnCreateFile(package);
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
