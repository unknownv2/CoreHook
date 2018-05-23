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
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using CoreHook;
using CoreHook.IPC.Pipes.Client;

namespace CoreHook.UWP.FileMonitor.Hook
{
    public class Library : IEntryPoint
    {
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

        private static readonly IJsonRpcContractResolver myContractResolver = new JsonRpcContractResolver
        {
            // Use camelcase for RPC method names.
            NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
            // Use camelcase for the property names in parameter value objects
            ParameterValueConverter = new CamelCaseJsonValueConverter()
        };

        private static void ClientWriteLine(object msg)
        {
            Debug.WriteLine(msg);
        }

        public void StartClient(string pipeName)
        {
            var clientPipe = new ClientPipe(pipeName);

            var clientTask = RunClientAsync(clientPipe.Start());

            // Wait for the client to exit.
            clientTask.GetAwaiter().GetResult();
        }
        Stack<String> Queue = new Stack<String>();

        LocalHook CreateFileHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        delegate IntPtr DCreateFile2(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            UInt32 InCreationDisposition,
            IntPtr pCreateExParams);

        [DllImport("kernelbase.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true,
        CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFile2(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            UInt32 InCreationDisposition,
            IntPtr pCreateExParams);

        // this is where we are intercepting all file accesses!
        private static IntPtr CreateFile2_Hooked(
           String InFileName,
           UInt32 InDesiredAccess,
           UInt32 InShareMode,
           UInt32 InCreationDisposition,
           IntPtr pCreateExParams)
        { 
            ClientWriteLine(string.Format("Creating file: '{0}'...", InFileName));

            try
            {
                Library This = (Library)HookRuntimeInfo.Callback;
                if (This != null)
                {
                    lock (This.Queue)
                    {
                        This.Queue.Push(InFileName);
                    }
                }
            }
            catch
            {

            }

            // call original API...
            return CreateFile2(
                InFileName,
                InDesiredAccess,
                InShareMode,
                InCreationDisposition,
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

            CreateFileHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
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

                var proxy = builder.CreateProxy<Shared.IFileMonitor>(client);

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
