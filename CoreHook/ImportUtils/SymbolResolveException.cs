using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.ImportUtils
{
    public class SymbolResolveException : Exception
    {
        public SymbolResolveException(string symbol, string message)
                    : base($"Failed to resolve {symbol} with {message}")
        {
        }
    }
}
