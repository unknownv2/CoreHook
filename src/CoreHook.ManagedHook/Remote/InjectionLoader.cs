using System;
using System.Collections.Generic;
using System.Threading;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

namespace CoreHook.ManagedHook.Remote
{
    public class InjectionHelper
    {
        internal class PipeConnectionBrokenException : Exception
        {
            internal PipeConnectionBrokenException() : base ($"Pipe connection was broken")
            {

            }
        }
        private class InjectionWait
        {
            public Mutex ThreadLock = new Mutex(false);
            public ManualResetEvent Completion = new ManualResetEvent(false);
            public Exception Error = null;
        }

        public static INamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleRequest);
        }

        private static void HandleRequest(string request, IPC.IConnection connection)
        {
            if(!connection.IsConnected)
            {
                throw new PipeConnectionBrokenException();
            }

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

        private static SortedList<int, InjectionWait> InjectionList = new SortedList<int, InjectionWait>();

        public static void BeginInjection(int targetPID)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                if (!InjectionList.TryGetValue(targetPID, out waitInfo))
                {
                    waitInfo = new InjectionWait();

                    InjectionList.Add(targetPID, waitInfo);
                }
            }

            waitInfo.ThreadLock.WaitOne();
            waitInfo.Error = null;
            waitInfo.Completion.Reset();

            lock (InjectionList)
            {
                if (!InjectionList.ContainsKey(targetPID))
                {
                    InjectionList.Add(targetPID, waitInfo);
                }
            }
        }

        public static void EndInjection(int targetPID)
        {
            lock (InjectionList)
            {
                InjectionList[targetPID].ThreadLock.ReleaseMutex();

                InjectionList.Remove(targetPID);
            }
        }

        public static void WaitForInjection(int targetPID, int timeOutMs = 20000)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[targetPID];
            }

            if (!waitInfo.Completion.WaitOne(timeOutMs, false))
            {
                throw new TimeoutException("Unable to wait for injection completion.");
            }

            if (waitInfo.Error != null)
            {
                throw waitInfo.Error;
            }
        }

        public static void InjectionException(int clientPID, Exception e)
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
