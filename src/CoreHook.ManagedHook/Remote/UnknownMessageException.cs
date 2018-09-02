using System;

namespace CoreHook.ManagedHook.Remote
{
    internal class UnknownMessageException : Exception
    {
        internal UnknownMessageException(string message)
            : base($"Unknown message type {message}.")
        {
        }
    }
}
