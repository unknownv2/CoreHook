using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.ManagedHook.ProcessUtils
{
    public class ProcessHelper
    {
        public readonly static Boolean Is64Bit = IntPtr.Size == 8;

        public static Process[] GetProcessListByName(string processName)
        {
            return Process.GetProcessesByName(processName);
        }
        public static Process GetProcessById(int processId)
        {
            return Process.GetProcessById(processId);
        }
        public static Process GetProcessByName(string processName)
        {
            return GetProcessListByName(processName)[0];
        }

        /// <summary>
        /// Returns the current native system process ID.
        /// </summary>
        /// <returns>The native system process ID.</returns>
        public static Int32 GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }
    }
}
