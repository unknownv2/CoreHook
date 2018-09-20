using System;

namespace CoreHook.ManagedHook.Remote
{
    internal class ProcessStartException : Exception
    {
        internal ProcessStartException(string processName)
                    : base($"Failed to start process {processName}.")
        {
        }
    }
}
