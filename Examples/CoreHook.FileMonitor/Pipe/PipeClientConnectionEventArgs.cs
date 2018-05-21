using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace CoreHook.FileMonitor.Pipe
{
    public class PipeClientConnectionEventArgs : EventArgs
    {
        public PipeClientConnectionEventArgs(NamedPipeServerStream pipeStream)
        {
            this.PipeStream = pipeStream;
        }
        public NamedPipeServerStream PipeStream { get; set; }
    }
    public interface ILog
    {
        void Debug(string formattedMessage, params object[] args);
        void Info(string formattedMessage, params object[] args);
        void Warn(string formattedMessage, params object[] args);
        void Error(string formattedMessage, params object[] args);
        void Fatal(string formattedMessage, params object[] args);
    }
    public interface IStats
    {
        void Log(string name, float value);
        void Log(string category, string name, float value);
        void LogSys();
    }
    internal class NullLogger : ILog
    {
        public void Debug(string formattedMessage, params object[] args)
        {
        }

        public void Info(string formattedMessage, params object[] args)
        {
        }

        public void Warn(string formattedMessage, params object[] args)
        {
        }

        public void Error(string formattedMessage, params object[] args)
        {
        }

        public void Fatal(string formattedMessage, params object[] args)
        {
        }
    }
    internal class NullStats : IStats
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
