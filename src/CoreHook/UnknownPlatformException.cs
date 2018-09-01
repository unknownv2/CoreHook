using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook
{
    internal class UnknownPlatformException : Exception
    {
        internal UnknownPlatformException()
                    : base("Failed to determine OS platform.")
        {
        }
    }
}
