// From https://github.com/Microsoft/VFSForGit/tree/master/GVFS/GVFS.Common/NamedPipes
using System;
using System.IO;

namespace CoreHook.IPC.NamedPipes
{
    public class BrokenPipeException : Exception
    {
        public BrokenPipeException(string message, IOException innerException)
            : base(message, innerException)
        {
        }
    }
}
