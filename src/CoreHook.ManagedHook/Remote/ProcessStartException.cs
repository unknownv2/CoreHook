using System;

namespace CoreHook.ManagedHook.Remote
{
    public class ProcessStartException : Exception
    {
        public ProcessStartException(string processName)
                    : base($"Failed to start process {processName}.")
        {
        }
    }
}
