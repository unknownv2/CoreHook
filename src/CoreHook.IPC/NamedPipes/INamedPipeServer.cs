using System;

namespace CoreHook.IPC.NamedPipes
{
    public interface INamedPipeServer : IDisposable
    {
        void OpenListeningPipe();
    }
}
