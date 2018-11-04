using System;
using System.IO;

namespace CoreHook.IPC.NamedPipes
{
    internal class InvalidPipeOperationException : Exception
    {
        internal InvalidPipeOperationException() : base() { }
        internal InvalidPipeOperationException(string message) : base(message) { }
        internal InvalidPipeOperationException(string message, IOException innerException)
            : base(message, innerException) { }
    }
}
