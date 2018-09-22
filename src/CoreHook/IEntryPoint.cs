using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook
{
    /// <summary>
    /// An Interface that is implemented by every hooking module.
    /// CoreHook executes the 'Run' method of that class when loading
    /// the hook module in the process.
    /// </summary>
    public interface IEntryPoint
    {

    }
}
