using System;

namespace CoreHook.ImportUtils
{
    public interface ILibLoader
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
    }
}