using System;
using System.Collections.Generic;
using CoreHook.IPC;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;
using CoreHook.IPC.Platform;
using CoreHook.IPC.Transport;

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
        private static readonly SortedList<int, InjectionState> InjectionList = new SortedList<int, InjectionState>();

        public static INamedPipeServer CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleMessage);
        }
        private static void HandleMessage(IMessage message, ITransportChannel channel)
        {
            switch (message.Header)
            {
                case InjectionCompleteNotification.InjectionComplete:
                    var messageData = InjectionCompleteNotification.ParseMessage(message);
                    if (messageData.Completed)
                    {
                        InjectionCompleted(messageData.ProcessId);
                    }
                    else
                    {
                        throw new InjectionLoadException($"Injection into process {messageData.ProcessId} failed.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Message type {message.Header} is not supported");
            }
        }
        private static void HandleMessage(IMessage message, IConnection connection)
        {
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

        private static void HandleRequest(string request, IConnection connection)
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
            InjectionState waitInfo;

            lock (InjectionList)
            {
                if (!InjectionList.TryGetValue(targetProcessId, out waitInfo))
                {
                    waitInfo = new InjectionState();

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

        public static void InjectionCompleted(int remoteProcessId)
        {
            InjectionState waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[remoteProcessId];
            }

            waitInfo.Error = null;
            waitInfo.Completion.Set();
        }

        public static void WaitForInjection(int targetProcessId, int timeOutMs = 20000)
        {
            InjectionState waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[targetProcessId];
            }

            if (!waitInfo.Completion.WaitOne(timeOutMs, false))
            {
                HandleException(targetProcessId, new TimeoutException("Unable to wait for injection completion."));
            }

            if (waitInfo.Error != null)
            {
                throw waitInfo.Error;
            }
        }

        private static void HandleException(int remoteProcessId, Exception e)
        {
            InjectionState waitInfo;

            lock (InjectionList)
            {
                waitInfo = InjectionList[remoteProcessId];
            }

            waitInfo.Error = e;
            waitInfo.Completion.Set();
        }
    }
}
