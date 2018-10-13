using System;

namespace CoreHook.Unmanaged
{
    internal class ModuleNotFoundException : Exception
    {
        internal ModuleNotFoundException() { }

        internal ModuleNotFoundException(string message) : base(message) { }
        internal ModuleNotFoundException(string message, Exception innerException) { }
    }
}
