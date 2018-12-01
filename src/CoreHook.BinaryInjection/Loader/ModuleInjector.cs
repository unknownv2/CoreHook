using CoreHook.Memory;

namespace CoreHook.BinaryInjection.Loader
{
    internal class ModuleInjector : IModuleInjector
    {
        private readonly IProcessManager _processManager;

        internal ModuleInjector(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        /// <summary>
        /// Load a module into managed process.
        /// </summary>
        /// <param name="path">File path of the module to be loaded.</param>
        public void Inject(string path) => _processManager.InjectBinary(path);
    }
}
