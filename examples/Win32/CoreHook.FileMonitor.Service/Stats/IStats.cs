using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.FileMonitor.Service.Stats
{
    public interface IStats
    {
        void Log(string name, float value);
        void Log(string category, string name, float value);
        void LogSys();
    }
}
