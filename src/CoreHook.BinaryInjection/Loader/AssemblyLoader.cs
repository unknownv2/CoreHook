using System;
using CoreHook.Memory;

namespace CoreHook.BinaryInjection.Loader
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly IProcessManager _processManager;

        private readonly IModuleInjector _moduleInjector;

        private readonly IRemoteThreadCreator _threadCreator;

        internal AssemblyLoader(IProcessManager processManager)
        {
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            _moduleInjector = new ModuleInjector(processManager);
            _threadCreator = new RemoteThreadCreator(processManager);
        }

        public void LoadModule(string path) => _moduleInjector.Inject(path);

        public void CreateThread(IRemoteFunctionCall call, bool waitForThreadExit = true)
        {
            _threadCreator.ExecuteRemoteFunction(call, waitForThreadExit);
        }

        public IntPtr CopyMemory(byte[] buffer, int length)
        {
            return _processManager.CopyToProcess(buffer, length);
        }

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _processManager.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AssemblyLoader()
        {
            Dispose();
        }
    }
}
