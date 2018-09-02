// From https://github.com/Microsoft/VFSForGit/tree/master/GVFS/GVFS.Common/NamedPipes
using System;

namespace CoreHook.IPC.NamedPipes
{
    internal class PipeNameLengthException : Exception
    {
        internal PipeNameLengthException(string message)
            : base(message)
        {
        }
    }
}
