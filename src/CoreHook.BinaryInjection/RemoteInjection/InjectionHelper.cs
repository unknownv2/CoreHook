using System;
using System.Collections.Generic;
using System.Threading;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

namespace CoreHook.BinaryInjection.RemoteInjection
{
    /// <summary>
    /// Handles notifications from a target process related to the CoreHook boostrapping stage,
    /// which is handled by the CoreLoad module. The host process should either receive 
    /// a message about that the CoreHook plugin was successfully loaded or throw an
    /// exception after a certain amount of time when no message has been received.
    /// </summary>
    public class InjectionHelper
    {
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
                throw new InvalidOperationException("Pipe connection was broken while handling request");
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
                        throw new InjectionLoadException($"Injection into process {reqData.PID} failed.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Message type {message.Header} is not supported");
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
                InjectionException(targetPID, new TimeoutException("Unable to wait for injection completion."));
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
}
