using System;
using System.Runtime.InteropServices;

namespace CoreHook.ImportUtils
{
    public class LibLoaderWindows : ILibLoader
    {
        void ILibLoader.FreeLibrary(IntPtr handle)
        {
            FreeLibrary(handle);
        }

        IntPtr ILibLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            return GetProcAddress(dllHandle, name);
        }

        IntPtr ILibLoader.LoadLibrary(string fileName)
        {
            return LoadLibrary(fileName);
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern int FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);
    }
}