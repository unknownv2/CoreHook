using System;

namespace CoreHook.IPC.NamedPipes
{
    internal class PipeNameLengthException : Exception
    {
        internal PipeNameLengthException(string message)
            : base(message)
        {
        }
    }
}
