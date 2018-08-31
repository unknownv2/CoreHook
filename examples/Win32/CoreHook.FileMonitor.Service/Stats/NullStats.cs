using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.FileMonitor.Service.Stats
{
    public class NullStats : IStats
    {
        public void Log(string name, float value)
        {
        }

        public void Log(string category, string name, float value)
        {
        }

        public void LogSys()
        {
        }
    }
}
