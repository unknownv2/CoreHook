using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace CoreHook.FileMonitor.Service.Pipe
{
    public class PipeClientConnectionEventArgs : EventArgs
    {
        public NamedPipeServerStream PipeStream { get; set; }

        public PipeClientConnectionEventArgs(NamedPipeServerStream pipeStream)
        {
            PipeStream = pipeStream;
        }
    }
}
