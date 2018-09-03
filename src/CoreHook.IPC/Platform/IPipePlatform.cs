// From https://github.com/Microsoft/VFSForGit/tree/master/GVFS/GVFS.Common/NamedPipes
using System.IO.Pipes;

namespace CoreHook.IPC.Platform
{
    public interface IPipePlatform
    {
        NamedPipeServerStream CreatePipeByName(string pipeName);
    }
}
