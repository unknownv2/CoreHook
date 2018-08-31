using System;
using System.Collections.Generic;
using System.Threading;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

namespace CoreHook.ManagedHook.Remote
{
    public class InjectionHelper
    {
        private class InjectionLoadException : Exception
        {
            public InjectionLoadException(int pid)
            : base($"Injection into process {pid} failed.")
            {
            }
        }
        private class UnknownMessageException : Exception
        {
            public UnknownMessageException(string message)
            : base($"Unknown message type {message}.")
            {
            }
        }

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
            var message = NamedPipeMessages.Message.FromString(request);

            switch (message.Header)
            {
                case NamedPipeMessages.InjectionCompleteNotification.InjectionComplete:
                    var msg = new NamedPipeMessages.InjectionCompleteNotification(message.Body);
                    var reqData = msg.RequestData;
                    if (reqData.Completed)
                    {
                        InjectionCompleted(reqData.PID);
                    }
                    else
                    {
                        throw new InjectionLoadException(reqData.PID);
                    }
                    break;
                default:
                    throw new UnknownMessageException(message.Header);
            }
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
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[InTargetPID];
            }

            if (!waitInfo.Completion.WaitOne(20000, false))
            {
                throw new TimeoutException("Unable to wait for injection completion.");
            }

            if (waitInfo.Error != null)
                throw waitInfo.Error;
        }

        public static void InjectionException(
            int clientPID,
            Exception e)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[clientPID];
            }

            waitInfo.Error = e;
            waitInfo.Completion.Set();
        }

        public static void InjectionCompleted(int clientPID)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[clientPID];
            }

            waitInfo.Error = null;
            waitInfo.Completion.Set();
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
