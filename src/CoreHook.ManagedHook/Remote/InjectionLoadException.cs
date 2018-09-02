using System;

namespace CoreHook.ManagedHook.Remote
{
    internal class InjectionLoadException : Exception
    {
        internal InjectionLoadException(int pid)
            : base($"Injection into process {pid} failed.")
        {
        }
    }
}
