using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.Threading;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

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

        public static NamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleRequest);
        }

        private static void HandleRequest(string request, NamedPipeServer.Connection connection)
        {
            NamedPipeMessages.Message message = NamedPipeMessages.Message.FromString(request);

            switch (message.Header)
            {
                case NamedPipeMessages.InjectionCompleteNotification.InjectionComplete:
                    var msg = new NamedPipeMessages.InjectionCompleteNotification(message.Body);
                    var reqData = msg.RequestData;
                    if (reqData.Completed)
                    {
                        InjectionCompleted(reqData.PID);
                    }
                    break;
            }
        }

        public static bool SendInjectionComplete(string pipeName, int pid)
        {
            using (NamedPipeClient pipeClient = new NamedPipeClient(pipeName))
            {
                if (pipeClient.Connect())
                {
                    var request = new NamedPipeMessages.InjectionCompleteNotification(pid, true);
                    if (pipeClient.TrySendRequest(request.CreateMessage()))
                    {
                        return true;
                    }
                }
            }

            return false;
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
        public static void EndInjection(int InTargetPID)
        {
            lock (InjectionList)
            {
                InjectionList[InTargetPID].ThreadLock.ReleaseMutex();

                InjectionList.Remove(InTargetPID);
            }
        }
        public static void WaitForInjection(int InTargetPID)
        {
            InjectionWait WaitInfo;

            lock (InjectionList)
            {
                WaitInfo = InjectionList[InTargetPID];
            }

            if (!WaitInfo.Completion.WaitOne(20000, false))
                throw new TimeoutException("Unable to wait for injection completion.");

            if (WaitInfo.Error != null)
                throw WaitInfo.Error;
        }

        public static void InjectionException(
            int clientPID,
            Exception e)
        {
            InjectionWait WaitInfo;

            lock (InjectionList)
            {
                WaitInfo = InjectionList[clientPID];
            }

            WaitInfo.Error = e;
            WaitInfo.Completion.Set();
        }

        public static void InjectionCompleted(int clientPID)
        {
            InjectionWait WaitInfo;

            lock (InjectionList)
            {
                WaitInfo = InjectionList[clientPID];
            }

            WaitInfo.Error = null;
            WaitInfo.Completion.Set();
        }
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
