using System;
using System.Collections.Generic;
using System.Text;
using CoreHook.Memory;

namespace CoreHook.BinaryInjection.Loader
{
    internal class ModuleInjector : IModuleInjector
    {
        private IProcessManager _processManager;

        internal ModuleInjector(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        public void Inject(string path) => _processManager.InjectBinary(path);
    }
}
