using System;
using System.Collections.Generic;
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

        public static INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform)
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

        public static void BeginInjection(int targetProcessId)
        {
            InjectionState state;

            lock (InjectionList)
            {
                if (!InjectionList.TryGetValue(targetProcessId, out state))
                {
                    state = new InjectionState();

                    InjectionList.Add(targetProcessId, state);
                }
            }

            state.ThreadLock.WaitOne();
            state.Error = null;
            state.Completion.Reset();

            lock (InjectionList)
            {
                if (!InjectionList.ContainsKey(targetProcessId))
                {
                    InjectionList.Add(targetProcessId, state);
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
            InjectionState state;

            lock (InjectionList)
            {
                state = InjectionList[remoteProcessId];
            }

            state.Error = null;
            state.Completion.Set();
        }

        public static void WaitForInjection(int targetProcessId, int timeOutMilliseconds = 20000)
        {
            InjectionState state;

            lock (InjectionList)
            {
                state = InjectionList[targetProcessId];
            }

            if (!state.Completion.WaitOne(timeOutMilliseconds, false))
            {
                HandleException(targetProcessId, new TimeoutException("Unable to wait for plugin injection to complete."));
            }

            if (state.Error != null)
            {
                throw state.Error;
            }
        }

        private static void HandleException(int remoteProcessId, Exception e)
        {
            InjectionState state;

            lock (InjectionList)
            {
                state = InjectionList[remoteProcessId];
            }

            state.Error = e;
            state.Completion.Set();
        }
    }
}
