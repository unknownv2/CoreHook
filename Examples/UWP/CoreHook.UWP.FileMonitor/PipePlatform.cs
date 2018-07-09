using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreHook.IPC.Platform;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CoreHook.UWP.FileMonitor
{
    public class PipePlatform : IPipePlatform
    {
        private int _maxConnections = 254;

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

        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    _maxConnections,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    65536,
                    65536,
                    CreateUWPPipeSecurity()
                    );
        }
    }
}
