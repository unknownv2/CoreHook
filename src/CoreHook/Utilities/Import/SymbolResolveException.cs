using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Utilities.Import
{
    internal class SymbolResolveException : Exception
    {
        internal SymbolResolveException(string symbol, string message)
                    : base($"Failed to resolve {symbol} with {message}")
        {
        }
    }
}
