using System;
using System.Runtime.InteropServices;

namespace CoreHook.Unmanaged.Linux
{
    public static class Process
    {
        private const string LIBINJECT = "inject.so";

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_read(int targetPid, IntPtr addr, IntPtr data, int len);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_write(int targetPid, IntPtr addr, byte[] data, int len);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_detach(int targetPid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_cont(int targetPid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int ptrace_attach(int targetPid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int set_pid(IntPtr handle, int pid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int parse_elf(IntPtr handle);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int print_elf(IntPtr handle);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern IntPtr find_symbol(IntPtr handle,
         [MarshalAs(UnmanagedType.LPStr)] String symbolName,
         [MarshalAs(UnmanagedType.LPStr)] String libraryName = null);
    }
    public static class ProcessLibInjector
    {
        private const string LIBINJECT = "libinjector.so";

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injector_attach(ref IntPtr injectHandle, int pid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injector_inject(IntPtr injectHandle, string injectionLib);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injector_detach(IntPtr injectHandle);
    }
}
