using CoreHook.IPC;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;

namespace CoreHook.CoreLoad
{
    internal class NotificationHelper
    {
        private readonly INamedPipe _pipe;

        internal NotificationHelper(string pipeName)
        {
            _pipe = CreateClient(pipeName);
            _pipe.Connect();
        }

        /// <summary>
        /// Notify the injecting process when injection has completed successfully
        /// and the plugin is about to be executed.
        /// </summary>
        /// <param name="pipeName">The notification pipe created by the remote process.</param>
        /// <param name="processId">The process ID to send in the notification message.</param>
        /// <returns>True if the injection completion notification was sent successfully.</returns>
        internal static bool SendInjectionComplete(string pipeName, int processId)
        {
            using (var pipeClient = CreateClient(pipeName))
            {
                if (pipeClient.Connect())
                {
                    return SendInjectionComplete(pipeClient.MessageHandler, processId);
                }
            }
            return false;
        }
        internal static bool SendLogMessage(string pipeName, string message, LogLevel level = LogLevel.Info)
        {
            using (var pipeClient = CreateClient(pipeName))
            {
                if (pipeClient.Connect())
                {
                    return SendLogMessage(pipeClient.MessageHandler, level, message);
                }
            }
            return false;
        }

        internal bool SendInjectionComplete(int processId)
        {
            return SendInjectionComplete(_pipe.MessageHandler, processId);
        }

        private static bool SendInjectionComplete(IMessageWriter writer, int processId)
        {
            return writer.TryWrite(InjectionCompleteNotification.CreateMessage(processId, true));
        }

        private static INamedPipe CreateClient(string pipeName)
        {
            return new NamedPipeClient(pipeName);
        }

        private static bool SendLogMessage(IMessageWriter writer, LogLevel level, string message)
        {
            return writer.TryWrite(LogMessageNotification.CreateMessage(level, message));
        }

        internal bool Log(string message, LogLevel level = LogLevel.Info)
        {
            return SendLogMessage(_pipe.MessageHandler, level, message);
        }
    }
}
