using System;
using CoreHook.Unmanaged.Windows;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;

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

    }
}
