using System;
using System.Reflection;

namespace CoreHook.CoreLoad
{
    internal interface IDependencyResolver : IDisposable
    {
        Assembly Assembly { get; }
    }
}
