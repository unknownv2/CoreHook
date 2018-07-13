using System;
using System.Runtime.InteropServices;

namespace CoreHook.Unmanaged.MacOS
{
    public static class Process
    {
        internal const string LIBINJECT = "libinject.dylib";

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injectByName(string targetName, string injectionLib, ref int pid);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int findProcessByName(string targetName);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int injectByPid(int targetPid, string injectionLib);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern IntPtr copyMemToProcess(string processName, byte[] data, long dataSize);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern IntPtr copyMemToProcessByPid(int targetPid, byte[] data, long dataSize);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern IntPtr injectByPidWithArgs(int targetPid, byte[] parameters, long paramsSize);

        [DllImport(LIBINJECT, SetLastError = true)]
        public static extern int freeProcessMemByPid(int targetPid, IntPtr address, long size);
    }
}
