using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.DependencyModel
{
    internal interface IDirectory
    {
        bool Exists(string path);
    }
}
