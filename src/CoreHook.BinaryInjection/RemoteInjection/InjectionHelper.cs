using System;
using System.Collections.Generic;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;

namespace CoreHook.BinaryInjection.RemoteInjection
{
    /// <summary>
    /// Handles notifications from a target process related to the CoreHook bootstrapping stage,
    /// which is handled by the CoreLoad module. The host process should either receive 
    /// a message about that the CoreHook plugin was successfully loaded or throw an
    /// exception after a certain amount of time when no message has been received.
    /// </summary>
    public class InjectionHelper
    {
        private static readonly SortedList<int, InjectionWait> InjectionList = new SortedList<int, InjectionWait>();

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

            var message = Message.FromString(request);

            switch (message.Header)
            {
                case InjectionCompleteNotification.InjectionComplete:
                    var reqData = new InjectionCompleteNotification(message.Body).RequestData;
                    if (reqData.Completed)
                    {
                        InjectionCompleted(reqData.ProcessId);
                    }
                    else
                    {
                        throw new InjectionLoadException($"Injection into process {reqData.ProcessId} failed.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Message type {message.Header} is not supported");
            }
        }

        public static void BeginInjection(int targetProcessId)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                if (!InjectionList.TryGetValue(targetProcessId, out waitInfo))
                {
                    waitInfo = new InjectionWait();

                    InjectionList.Add(targetProcessId, waitInfo);
                }
            }

            waitInfo.ThreadLock.WaitOne();
            waitInfo.Error = null;
            waitInfo.Completion.Reset();

            lock (InjectionList)
            {
                if (!InjectionList.ContainsKey(targetProcessId))
                {
                    InjectionList.Add(targetProcessId, waitInfo);
                }
            }
        }

        public static void EndInjection(int targetProcessId)
        {
            lock (InjectionList)
            {
                InjectionList[targetProcessId].ThreadLock.ReleaseMutex();

                InjectionList.Remove(targetProcessId);
            }
        }

        public static void WaitForInjection(int targetProcessId, int timeOutMs = 20000)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[targetProcessId];
            }

            if (!waitInfo.Completion.WaitOne(timeOutMs, false))
            {
                InjectionException(targetProcessId, new TimeoutException("Unable to wait for injection completion."));
            }

            if (waitInfo.Error != null)
            {
                throw waitInfo.Error;
            }
        }

        public static void InjectionException(int remoteProcessId, Exception e)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[remoteProcessId];
            }

            waitInfo.Error = e;
            waitInfo.Completion.Set();
        }

        public static void InjectionCompleted(int remoteProcessId)
        {
            InjectionWait waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[remoteProcessId];
            }

            waitInfo.Error = null;
            waitInfo.Completion.Set();
        }
    }
}
