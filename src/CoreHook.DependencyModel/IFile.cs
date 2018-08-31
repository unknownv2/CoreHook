using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CoreHook.DependencyModel
{
    internal interface IFile
    {
        bool Exists(string path);

        string ReadAllText(string path);

        Stream OpenRead(string path);

        Stream OpenFile(
            string path,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions);

        void CreateEmptyFile(string path);
    }
}
