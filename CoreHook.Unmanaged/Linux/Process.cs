using System;
using System.Runtime.InteropServices;

namespace CoreHook.Unmanaged.Linux
{
    public static class Process
    {
        internal const string LIBINJECT = "inject.so";

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injectByName(string targetName, string injectionLib, ref int pid);
        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int findProcessByName(string targetName);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injectByPid(int targetPid, string injectionLib);
        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_read(int targetPid, long addr, IntPtr vptr, int len);
        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern void ptrace_write(int targetPid, long addr, byte[] vptr, int len);
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
         [MarshalAs(UnmanagedType.LPStr)] String libraryName);
    }
}
