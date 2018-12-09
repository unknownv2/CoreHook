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
        private static readonly SortedList<int, InjectionState> ProcessList = new SortedList<int, InjectionState>();

        /// <summary>
        /// Create a named pipe server for awaiting messages.
        /// </summary>
        /// <param name="namedPipeName">The name of the server pipe.</param>
        /// <param name="pipePlatform">A class that creates a pipe instance.</param>
        /// <returns>The named pipe server.</returns>
        public static INamedPipe CreateServer(string namedPipeName, IPipePlatform pipePlatform)
        {
            return NamedPipeServer.StartNewServer(namedPipeName, pipePlatform, HandleMessage);
        }

        /// <summary>
        /// Process a message received by the server.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="channel">The server communication channel.</param>
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
                {
                    throw new InvalidOperationException($"Message type {message.Header} is not supported");
                }
            }
        }

        /// <summary>
        /// Start the process for awaiting a notification from a remote process.
        /// </summary>
        /// <param name="targetProcessId">The remote process ID we expect the notification from.</param>
        public static void BeginInjection(int targetProcessId)
        {
            InjectionState state;

            lock (ProcessList)
            {
                if (!ProcessList.TryGetValue(targetProcessId, out state))
                {
                    state = new InjectionState();

                    ProcessList.Add(targetProcessId, state);
                }
            }

            state.ThreadLock.WaitOne();
            state.Error = null;
            state.Completion.Reset();

            lock (ProcessList)
            {
                if (!ProcessList.ContainsKey(targetProcessId))
                {
                    ProcessList.Add(targetProcessId, state);
                }
            }
        }

        /// <summary>
        /// Remove a process ID from a list we use to wait for remote notifications.
        /// </summary>
        /// <param name="targetProcessId">The remote process ID we expect the notification from.</param>
        public static void EndInjection(int targetProcessId)
        {
            lock (ProcessList)
            {
                ProcessList[targetProcessId].ThreadLock.ReleaseMutex();

                ProcessList.Remove(targetProcessId);
            }
        }

        /// <summary>
        /// Complete the wait for a notification.
        /// </summary>
        /// <param name="remoteProcessId">The remote process ID we expect the notification from.</param>
        public static void InjectionCompleted(int remoteProcessId)
        {
            InjectionState state;

            lock (ProcessList)
            {
                state = ProcessList[remoteProcessId];
            }

            state.Error = null;
            state.Completion.Set();
        }

        /// <summary>
        /// Block the current thread and wait to until we receive a signal from a remote process to continue.
        /// </summary>
        /// <param name="targetProcessId">The remote process ID we expect the notification from.</param>
        /// <param name="timeOutMilliseconds">The time in milliseconds to wait for the notification message.</param>
        public static void WaitForInjection(int targetProcessId, int timeOutMilliseconds = 20000)
        {
            InjectionState state;

            lock (ProcessList)
            {
                state = ProcessList[targetProcessId];
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

        /// <summary>
        /// Handle any errors that occur during the wait for the injection complete notification.
        /// If an error occurs, then save it and complete the notification wait so the host program
        /// can continue the execution of the thread being blocked.
        /// </summary>
        /// <param name="remoteProcessId">The process ID we expect a notification from.</param>
        /// <param name="e">The error that occured during the wait.</param>
        private static void HandleException(int remoteProcessId, Exception e)
        {
            InjectionState state;

            lock (ProcessList)
            {
                state = ProcessList[remoteProcessId];
            }

            state.Error = e;
            state.Completion.Set();
        }
    }
}
