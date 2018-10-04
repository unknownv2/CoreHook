using System;

namespace CoreHook.ManagedHook.Remote
{
    internal class InjectionLoadException : Exception
    {
        internal InjectionLoadException() { }
        internal InjectionLoadException(string message) { }
        internal InjectionLoadException(string message, Exception innerException) {  }
    }
}
