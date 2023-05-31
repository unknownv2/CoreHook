using System;

namespace CoreHook.BinaryInjection;

internal class InjectionLoadException : Exception
{
    internal InjectionLoadException() { }
    internal InjectionLoadException(string message) { }
    internal InjectionLoadException(string message, Exception innerException) { }
}
