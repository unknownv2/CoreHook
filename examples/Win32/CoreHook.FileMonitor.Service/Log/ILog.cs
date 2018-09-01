using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.FileMonitor.Service.Log
{
    public interface ILog
    {
        void Debug(string formattedMessage, params object[] args);
        void Info(string formattedMessage, params object[] args);
        void Warn(string formattedMessage, params object[] args);
        void Error(string formattedMessage, params object[] args);
        void Fatal(string formattedMessage, params object[] args);
    }
}
