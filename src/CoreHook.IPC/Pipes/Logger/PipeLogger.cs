using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CoreHook.IPC.NamedPipes;

namespace CoreHook.IPC.Pipes.Logger
{
    [StructLayout(LayoutKind.Sequential)]
    struct LogMessage
    {
        public ushort nBytes;
        public byte nFacility;
        public byte nSeverity;
        public int nProcessId;
        public long ftOccurance;
        public int fTerminate;
        public string szMessage;
    }

    public class PipeLogger
    {
        private NamedPipeClient _client;

        public PipeLogger(string pipeName)
        {
            _client = new NamedPipeClient(pipeName);
        }
        private void Log(string message)
        {
            _client.SendRequest(StructToByteArray(new LogMessage()
            {
                nBytes = (ushort)(20 + message.Length),
                nFacility = 0x50,
                nSeverity = 0x60,
                nProcessId = 1,
                ftOccurance = DateTime.Now.ToFileTime(),
                fTerminate = 0,
                szMessage = message
            }));
        }
        public static byte[] StructToByteArray(object obj, int length = -1)
        {
            var len = Marshal.SizeOf(obj);
            var arr = new byte[length == -1 ? len : length];

            var ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}
