using System;
using System.Collections.Generic;
using System.Text;
using CoreHook.IPC.Platform;
using System.IO;
using System.IO.Pipes;

namespace CoreHook.Tests
{
    public class PipePlatformBase : IPipePlatform
    {
        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {
            return new NamedPipeServerStream(
             pipeName,
             PipeDirection.InOut,
             NamedPipeServerStream.MaxAllowedServerInstances,
             PipeTransmissionMode.Byte,
             PipeOptions.Asynchronous,
             65536,
             65536
             );
        }
    }
}
