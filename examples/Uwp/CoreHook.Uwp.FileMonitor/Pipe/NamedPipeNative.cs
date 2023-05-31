using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

using Microsoft.Win32.SafeHandles;

using static Interop.Kernel32;

namespace CoreHook.Uwp.FileMonitor.Pipe;

internal static class NamedPipeNative
{
    internal static SECURITY_ATTRIBUTES GetSecurityAttributes(GCHandle securityDescriptorHandle, Interop.BOOL inheritHandle = Interop.BOOL.FALSE)
    {
        SECURITY_ATTRIBUTES securityAttributes = new SECURITY_ATTRIBUTES
        {
            bInheritHandle = inheritHandle,
            nLength = (uint)Marshal.SizeOf<SECURITY_ATTRIBUTES>(),
            lpSecurityDescriptor = securityDescriptorHandle.AddrOfPinnedObject()
        };
        return securityAttributes;
    }

    internal static NamedPipeServerStream CreateNamedServerPipe(string serverName, string namespaceName, string pipeName, PipeSecurity pipeSecurity)
    {
        string fullPipeName = $@"\\{serverName}\{namespaceName}\{pipeName}";
        var securityDescriptor = new CommonSecurityDescriptor(false, false, pipeSecurity.GetSecurityDescriptorBinaryForm(), 0);

        byte[] securityDescriptorBuffer = new byte[securityDescriptor.BinaryLength];
        securityDescriptor.GetBinaryForm(securityDescriptorBuffer, 0);

        GCHandle? securityDescriptorHandle = GCHandle.Alloc(securityDescriptorBuffer, GCHandleType.Pinned);
        var securityAttributes = GetSecurityAttributes(securityDescriptorHandle.Value);

        if (WaitNamedPipe(fullPipeName, System.Threading.Timeout.Infinite))
        {
            if (Marshal.GetLastWin32Error() != Interop.Errors.ERROR_FILE_NOT_FOUND)
            {
                throw new InvalidOperationException();
            }
        }

        SafePipeHandle pipeHandle = CreateNamedPipe(
            fullPipeName,
            Interop.Kernel32.PipeOptions.PIPE_ACCESS_DUPLEX | FileOperations.FILE_FLAG_OVERLAPPED,
            Interop.Kernel32.PipeOptions.PIPE_TYPE_BYTE | Interop.Kernel32.PipeOptions.PIPE_READMODE_BYTE,
            1,
            65536,
            65536,
            0,
            ref securityAttributes);

        securityDescriptorHandle.Value.Free();

        if (pipeHandle.IsInvalid)
        {
            throw new InvalidOperationException();
        }

        try
        {
            return new NamedPipeServerStream(PipeDirection.InOut, true, true, pipeHandle);
        }
        catch (Exception)
        {
            pipeHandle.Dispose();
            throw;
        }
    }
}
