using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using CoreHook.IPC.Platform;

namespace CoreHook.Uwp.FileMonitor.Pipe;

public class PipePlatform : IPipePlatform
{
    /// <summary>
    /// Create the pipe security rules required for communicating with UWP applications.
    /// </summary>
    /// <returns>The pipe access control for communicating with UWP applications.</returns>
    private static PipeSecurity CreateUwpPipeSecurity()
    {
        const PipeAccessRights pipeAccess = PipeAccessRights.ReadWrite;
        const AccessControlType accessControl = AccessControlType.Allow;

        var pipeSecurity = new PipeSecurity();

        using (var identity = WindowsIdentity.GetCurrent())
        {
            pipeSecurity.AddAccessRule(new PipeAccessRule(identity.User, pipeAccess, accessControl));
            if (identity.User != identity.Owner)
            {
                pipeSecurity.AddAccessRule(new PipeAccessRule(identity.Owner, pipeAccess, accessControl));
            }
        }

        pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), pipeAccess, accessControl));

        pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), pipeAccess, accessControl));
        pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), pipeAccess, accessControl));

        return pipeSecurity;
    }

    /// <summary>
    /// Create a named pipe server that allows communicating with UWP applications.
    /// </summary>
    /// <param name="pipeName">The name of the pipe to create.</param>
    /// <param name="serverName">The name of the remote computer, or "." to specify the local computer.</param>
    /// <returns>The named pipe used for communicating with UWP applications.</returns>
    public NamedPipeServerStream CreatePipeByName(string pipeName, string serverName = ".")
    {
        return NamedPipeNative.CreateNamedServerPipe(serverName, "pipe", pipeName, CreateUwpPipeSecurity());
    }
}
