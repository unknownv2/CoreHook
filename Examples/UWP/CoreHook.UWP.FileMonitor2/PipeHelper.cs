using System;
using System.Collections.Generic;
using System.Text;
using CoreHook.FileMonitor.Service;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO.Pipes;
using System.IO;

namespace CoreHook.UWP.FileMonitor2
{
    public class PipeHelper
    {
        internal static partial class Libraries
        {
            internal const string Kernel32 = "kernel32.dll";
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

        private string PipeName;

        public PipeHelper(string pipeName)
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
            await Task.Yield();

            CreateHooks();

            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private static void Log(object msg)
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
            Log($"Creating pipe: '{pipeName}'...");

            try
            {
                PipeHelper This = (PipeHelper)HookRuntimeInfo.Callback;
                if (This != null)
                {

                    if (pipeName.Contains(This.PipeName))
                    {
                        var pinningHandle = new GCHandle();
                        IntPtr result = IntPtr.Zero;
                        try
                        {
                            var security = GetSecAttrs(
                                HandleInheritability.None,
                                CreateUWPPipeSecurity(),
                                ref pinningHandle);

                            Log("Returning modified pipe");

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
            }
            catch (Exception ex)
            {
                Log(ex);
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
            Log("Adding hook to kernel32.dll!CreateNamedPipeW");

            CreateNamedPipeHook = LocalHook.Create(
                LocalHook.GetProcAddress(Libraries.Kernel32, "CreateNamedPipeW"),
                new DCreateNamedPipe(CreateNamedPipe_Hooked),
                this);

            CreateNamedPipeHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

            Started = true;
        }
    }
}
