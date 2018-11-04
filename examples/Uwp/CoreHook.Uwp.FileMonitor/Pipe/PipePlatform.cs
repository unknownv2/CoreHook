
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using CoreHook.IPC.Platform;

namespace CoreHook.Uwp.FileMonitor.Pipe
{
    public class PipePlatform : IPipePlatform
    {
        private static PipeSecurity CreateUWPPipeSecurity()
        {
            const PipeAccessRights pipeAccess = PipeAccessRights.ReadWrite;
            const AccessControlType accessControl = AccessControlType.Allow;
            var pipeSecurity = new PipeSecurity();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                pipeSecurity.AddAccessRule(
                      new PipeAccessRule(identity.User, pipeAccess, accessControl)
                );
                if (identity.User != identity.Owner)
                {
                    pipeSecurity.AddAccessRule(
                        new PipeAccessRule(identity.Owner, pipeAccess, accessControl)
                    );
                }
                pipeSecurity.AddAccessRule(new PipeAccessRule(
                    identity.User, pipeAccess, accessControl));
 
            }
            pipeSecurity.AddAccessRule(
                new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), pipeAccess, accessControl)
            );

            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), pipeAccess, accessControl));

            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), pipeAccess, accessControl));

            return pipeSecurity;
        }

        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return NamedPipeNative.CreateNamedServerPipe(".", "pipe", pipeName, CreateUWPPipeSecurity());
        }
    }
}
