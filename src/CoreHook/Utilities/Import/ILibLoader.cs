using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.Utilities.Import
{
    public interface ILibLoader
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
    }
}
