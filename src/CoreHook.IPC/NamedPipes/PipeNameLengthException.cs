using System;

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
