using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoreHook.Examples.Common
{
    public interface ISessionFeature
    {
        CancellationToken CancellationToken { get; }

        void StopServer();
    }
}
