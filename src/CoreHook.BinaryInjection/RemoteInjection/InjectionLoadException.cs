using System;

namespace CoreHook.BinaryInjection.RemoteInjection
{
    internal class InjectionLoadException : Exception
    {
        internal InjectionLoadException() { }
        internal InjectionLoadException(string message) { }
        internal InjectionLoadException(string message, Exception innerException) {  }
    }
}
