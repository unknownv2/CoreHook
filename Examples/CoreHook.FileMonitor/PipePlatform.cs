using System;
using System.Collections.Generic;
using System.Text;
using CoreHook.IPC.Platform;
using System.IO.Pipes;

namespace CoreHook.FileMonitor
{
    public class PipePlatform : IPipePlatform
    {
        public NamedPipeServerStream CreatePipeByName(string pipeName)
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
