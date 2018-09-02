using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection
{
    internal class BinaryLoaderException : Exception
    {
        internal BinaryLoaderException(string error)
                    : base(error)
        {
        }
    }
}
