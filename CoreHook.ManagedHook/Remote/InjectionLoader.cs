using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.Threading;

namespace CoreHook.ManagedHook.Remote
{
    public class InjectionHelper
    {
        private class InjectionWait
        {
            public Mutex ThreadLock = new Mutex(false);
            public ManualResetEvent Completion = new ManualResetEvent(false);
            public Exception Error = null;
        }
        private static SortedList<Int32, InjectionWait> InjectionList = new SortedList<Int32, InjectionWait>();
        public static void BeginInjection(int InTargetPID)
        {
            InjectionWait WaitInfo;

            lock (InjectionList)
            {
                if (!InjectionList.TryGetValue(InTargetPID, out WaitInfo))
                {
                    WaitInfo = new InjectionWait();

                    InjectionList.Add(InTargetPID, WaitInfo);
                }
            }

            WaitInfo.ThreadLock.WaitOne();
            WaitInfo.Error = null;
            WaitInfo.Completion.Reset();

            lock (InjectionList)
            {
                if (!InjectionList.ContainsKey(InTargetPID))
                    InjectionList.Add(InTargetPID, WaitInfo);
            }
        }
    }
    public interface IInjectionHelper
    {

    }

    public class InjectionPrepareEventArgs : EventArgs
    {
        public string PipeName{ get; set; }

        public InjectionPrepareEventArgs(string pipeName)
        {
            PipeName = pipeName;
        }
    }
}
