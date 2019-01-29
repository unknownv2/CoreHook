using CoreHook.Memory.Processes;

namespace CoreHook.BinaryInjection.Loader
{
    internal class ModuleInjector : IModuleInjector
    {
        private readonly IModuleManager _moduleManager;

        internal ModuleInjector(IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        /// <summary>
        /// Load a module into the process controlled by the process manager.
        /// </summary>
        /// <param name="path">File path of the module to be loaded.</param>
        public void Inject(string path) => _moduleManager.LoadModule(path);
    }
}
