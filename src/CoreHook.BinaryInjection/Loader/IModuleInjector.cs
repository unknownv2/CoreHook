using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    interface IModuleInjector
    {
        void Inject(string path);
    }
}
