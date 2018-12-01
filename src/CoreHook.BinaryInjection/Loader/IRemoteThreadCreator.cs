using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook.BinaryInjection.Loader
{
    interface IRemoteThreadCreator
    {
        void ExecuteRemoteFunction(IRemoteFunctionCall call, bool waitForThreadExit);
    }
}
