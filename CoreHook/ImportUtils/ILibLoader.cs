using System;

namespace CoreHook.ImportUtils
{
    internal interface ILibLoader
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
    }
}