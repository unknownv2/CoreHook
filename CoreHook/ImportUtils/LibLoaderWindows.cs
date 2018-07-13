using System;
using System.Runtime.InteropServices;

namespace CoreHook.ImportUtils
{

    public class LibLoaderWindows : ILibLoader
    {
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        void ILibLoader.FreeLibrary(IntPtr handle)
        {
            FreeLibrary(handle);
        }

        IntPtr ILibLoader.GetProcAddress(IntPtr dllHandle, string name)
        {
            IntPtr address = GetProcAddress(dllHandle, name);
            if(address == IntPtr.Zero)
            {
                throw new SymbolResolveException(name, $"last-error code {GetLastError().ToString()}");
            }
            return address;
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