
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using CoreHook.IPC.Platform;
using CoreHook.UWP.FileMonitor.Pipe;

namespace CoreHook.UWP.FileMonitor
{
    /// <summary>
    /// Using code from here as an example:
    /// https://github.com/PowerShell/PowerShellEditorServices/blob/1031e2296449ab30bb4968e0285566a33e4bf9f4/src/PowerShellEditorServices.Protocol/MessageProtocol/Channel/NamedPipeServerListener.cs#L136-L274
    /// </summary>
    public class PipePlatform : IPipePlatform
    {
        private static PipeSecurity CreateUWPPipeSecurity()
        {
            const PipeAccessRights access = PipeAccessRights.ReadWrite;

            var pipeSecurity = new PipeSecurity();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                pipeSecurity.AddAccessRule(
                      new PipeAccessRule(identity.User, access, AccessControlType.Allow)
                  );
                if (identity.User != identity.Owner)
                {
                    pipeSecurity.AddAccessRule(
                        new PipeAccessRule(identity.Owner, access, AccessControlType.Allow)
                    );
                }
                // Allow the current user read/write access to the pipe.
                pipeSecurity.AddAccessRule(new PipeAccessRule(
                    identity.User, access, AccessControlType.Allow));
 
            }
            // And the user's Admin user.
            pipeSecurity.AddAccessRule(
                new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), access, AccessControlType.Allow)
            );

            // Allow everybody. This may or may not be changed later.
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), access, AccessControlType.Allow));

            // Allow remote connections.
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.RemoteLogonIdSid, null), access, AccessControlType.Allow));


            // Allow all app packages to connect.
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), access, AccessControlType.Allow));
            return pipeSecurity;
        }

        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            // Unfortunately, .NET Core does not support passing in a PipeSecurity object into the constructor for
            // NamedPipeServerStream so we are creating native Named Pipes and securing them using native APIs. The
            // issue on .NET Core regarding Named Pipe security is here: https://github.com/dotnet/corefx/issues/30170
            // 99% of this code was borrowed from PowerShell here:
            // https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/remoting/common/RemoteSessionNamedPipe.cs#L124-L256
            return NamedPipeNative.CreateNamedPipe(".", "pipe", pipeName, CreateUWPPipeSecurity());
        }
    }
}
