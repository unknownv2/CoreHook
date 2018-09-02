using System;

namespace CoreHook.ImportUtils
{
    internal class SymbolResolveException : Exception
    {
        internal SymbolResolveException(string symbol, string message)
                    : base($"Failed to resolve {symbol} with {message}")
        {
        }
    }
}
