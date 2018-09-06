using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;

namespace CoreHook.IPC.Pipes.Client
{
    public class ClientPipe
    {
        private const string _serverName = ".";

        private readonly string _pipeName;

        private NamedPipeClientStream _pipe;

        public ClientPipe(string pipe)
        {
            _pipeName = pipe;
        }
  
        public NamedPipeClientStream Start()
        {
            _pipe = CreatePipe(_pipeName, _serverName);

            try
            {
                _pipe.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return _pipe;
        }

        private static NamedPipeClientStream CreatePipe(string pipeName, string serverName)
        {
            return new NamedPipeClientStream(
              serverName,
              pipeName,
              PipeDirection.InOut,
              PipeOptions.Asynchronous,
              TokenImpersonationLevel.Impersonation);
        }

        public void Close()
        {
            if (_pipe != null)
            {
                _pipe.Close();
            }
        }
    }
}
