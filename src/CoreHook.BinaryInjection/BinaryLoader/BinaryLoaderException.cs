using System;

namespace CoreHook.BinaryInjection.BinaryLoader
{
    internal class BinaryLoaderException : Exception
    {
        internal BinaryLoaderException(string error)
                    : base(error)
        {
        }
    }
}
