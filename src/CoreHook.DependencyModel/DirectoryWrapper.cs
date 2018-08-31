using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CoreHook.DependencyModel
{
    internal class DirectoryWrapper : IDirectory
    {
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
