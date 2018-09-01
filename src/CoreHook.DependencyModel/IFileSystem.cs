using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.DependencyModel
{
    internal interface IFileSystem
    {
        IFile File { get; }
        IDirectory Directory { get; }
    }
}
