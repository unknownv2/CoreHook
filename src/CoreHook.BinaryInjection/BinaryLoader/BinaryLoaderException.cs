using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection
{
    internal class BinaryLoaderException : Exception
    {
        public BinaryLoaderException(string error)
                    : base(error)
        {
        }
    }
}
