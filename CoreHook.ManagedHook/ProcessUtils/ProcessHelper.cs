using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreHook.ManagedHook.ProcessUtils
{
    public class ProcessHelper
    {
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
            return NativeAPI.GetCurrentProcessId();
        }
    }
}
