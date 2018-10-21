using System;

namespace CoreHook.Memory
{
    internal class MemoryOperationException : Exception
    {
        internal MemoryOperationException() { }
        internal MemoryOperationException(string operation)
            : base($"Memory operation '{operation}' failed.") { }
        internal MemoryOperationException(string message, Exception innerException) { }
    }
}
