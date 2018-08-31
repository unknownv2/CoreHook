using System;

namespace CoreHook.ManagedHook.Remote
{
    public class UnsupportedPlatformException : Exception
    {
        public UnsupportedPlatformException(string operation)
                    : base($"Unsupported platform for {operation}.")
        {
        }
    }
}
