using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.IPC.NamedPipes
{
    public class PipeNameLengthException : Exception
    {
        public PipeNameLengthException(string message)
            : base(message)
        {
        }
    }
}
