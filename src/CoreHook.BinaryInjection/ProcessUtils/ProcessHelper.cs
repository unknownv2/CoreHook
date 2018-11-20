using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CoreHook.BinaryInjection.ProcessUtils
{
    public static class ProcessHelper
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
            return GetProcessListByName(processName).First();
        }
        public static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }
        public static bool IsArchitectureArm()
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        }
    }
}
