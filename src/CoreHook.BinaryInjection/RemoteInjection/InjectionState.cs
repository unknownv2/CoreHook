using System;
using System.Threading;

namespace CoreHook.BinaryInjection.RemoteInjection
{
    internal class InjectionState
    {
        public readonly Mutex ThreadLock = new Mutex(false);
        public readonly ManualResetEvent Completion = new ManualResetEvent(false);
        public Exception Error;
    }
}
