using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace CoreHook.Uwp.FileMonitor.Pipe
{
    internal static class NamedPipeNative
    {
        internal static Interop.Kernel32.SECURITY_ATTRIBUTES GetSecurityAttributes(
            GCHandle securityDescriptorHandle,
            Interop.BOOL inheritHandle = Interop.BOOL.FALSE)
        {
            Interop.Kernel32.SECURITY_ATTRIBUTES securityAttributes = new Interop.Kernel32.SECURITY_ATTRIBUTES();
            securityAttributes.bInheritHandle = inheritHandle;
            securityAttributes.nLength = (uint)Marshal.SizeOf(securityAttributes);
            securityAttributes.lpSecurityDescriptor = securityDescriptorHandle.AddrOfPinnedObject();
            return securityAttributes;
        }
        
        internal static NamedPipeServerStream CreateNamedServerPipe(
            string serverName,
            string namespaceName,
            string pipeName,
            PipeSecurity pipeSecurity)
        {

            string fullPipeName = $@"\\{serverName}\{namespaceName}\{pipeName}";
            var securityDescriptor = new CommonSecurityDescriptor(
                false,
                false,
                pipeSecurity.GetSecurityDescriptorBinaryForm(),
                0);

            Interop.Kernel32.SECURITY_ATTRIBUTES securityAttributes = new Interop.Kernel32.SECURITY_ATTRIBUTES();
            GCHandle? securityDescriptorHandle = null;
            if(securityDescriptor != null)
            {
                byte[] securityDescriptorBuffer = new byte[securityDescriptor.BinaryLength];
                securityDescriptor.GetBinaryForm(securityDescriptorBuffer, 0);

                securityDescriptorHandle = GCHandle.Alloc(securityDescriptorBuffer, GCHandleType.Pinned);
                securityAttributes = GetSecurityAttributes(securityDescriptorHandle.Value);
            }

            if(Interop.Kernel32.WaitNamedPipe(fullPipeName, System.Threading.Timeout.Infinite))
            { 
                if(Marshal.GetLastWin32Error() != Interop.Errors.ERROR_FILE_NOT_FOUND)
                {
                    throw new InvalidOperationException();
                }
            }

            SafePipeHandle pipeHandle = Interop.Kernel32.CreateNamedPipe(
                fullPipeName,
                Interop.Kernel32.PipeOptions.PIPE_ACCESS_DUPLEX | Interop.Kernel32.FileOperations.FILE_FLAG_OVERLAPPED,
                Interop.Kernel32.PipeOptions.PIPE_TYPE_BYTE | Interop.Kernel32.PipeOptions.PIPE_READMODE_BYTE,
                255,
                65536,
                65536,
                0,
                ref securityAttributes);

            if (securityDescriptorHandle != null)
            {
                securityDescriptorHandle.Value.Free();
            }

            if(pipeHandle.IsInvalid)
            {
                throw new InvalidOperationException();
            }

            try
            {
                return new NamedPipeServerStream(PipeDirection.InOut, true, true, pipeHandle);
            }
            catch(Exception)
            {
                pipeHandle.Dispose();
                throw;
            }
        }
    }
}
