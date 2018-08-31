using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.DependencyModel
{
    internal interface IEnvironment
    {
        string GetEnvironmentVariable(string name);
    }
}
