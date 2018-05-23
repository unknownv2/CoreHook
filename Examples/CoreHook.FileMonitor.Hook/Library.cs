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

namespace CoreHook.FileMonitor.Hook
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
        delegate IntPtr DCreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFile(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        // this is where we are intercepting all file accesses!
        private static IntPtr CreateFile_Hooked(
            String InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile)
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
            return CreateFile(
                InFileName,
                InDesiredAccess,
                InShareMode,
                InSecurityAttributes,
                InCreationDisposition,
                InFlagsAndAttributes,
                InTemplateFile);
        }

        private void CreateHooks()
        {
            ClientWriteLine("Adding hook to kernel32.dll!CreateFileW");

            CreateFileHook = LocalHook.Create(
                LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                new DCreateFile(CreateFile_Hooked),
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
