using System;

namespace CoreHook.BinaryInjection
{
    internal class MemoryOperationException : Exception
    {
        internal MemoryOperationException(string operation)
            : base($"Memory operation '{operation}' failed.")
        {
        }
    }
}
