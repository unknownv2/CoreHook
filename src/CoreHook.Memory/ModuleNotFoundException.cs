using System;

namespace CoreHook.Memory
{
    internal class ModuleNotFoundException : Exception
    {
        internal ModuleNotFoundException() { }

        internal ModuleNotFoundException(string message) : base(message) { }
        internal ModuleNotFoundException(string message, Exception innerException) { }
    }
}
