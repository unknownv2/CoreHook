using System;

namespace CoreHook.IPC.NamedPipes
{
    internal class PipeMessageLengthException : Exception
    {
        internal PipeMessageLengthException(string pipeName, int maxMessageLength)
            : base($"The message from pipe ({pipeName}) exceeded the max length allowed ({maxMessageLength}))")
        {
        }
        
    }
}
