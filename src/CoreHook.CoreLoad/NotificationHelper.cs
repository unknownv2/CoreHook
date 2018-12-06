using CoreHook.IPC;
using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;

namespace CoreHook.CoreLoad
{
    internal static class NotificationHelper
    {
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

        private static bool SendInjectionComplete(IMessageWriter writer, int processId)
        {
            return writer.TryWrite(InjectionCompleteNotification.CreateMessage(processId, true));
        }

        private static INamedPipeClient CreateClient(string pipeName)
        {
            return new NamedPipeClient(pipeName);
        }
    }
}
