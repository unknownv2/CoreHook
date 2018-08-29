using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoreHook.FileMonitor.Service
{
    public class FileMonitorSessionFeature
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> that can stop the server.
        /// </summary>
        public CancellationToken CancellationToken => cts.Token;

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void StopServer()
        {
            cts.Cancel();
        }
    }
}
