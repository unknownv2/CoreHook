using CoreHook.IPC.Messages;
using CoreHook.IPC.NamedPipes;

using System;

namespace CoreHook.Loader;

internal class NotificationHelper : IDisposable
{
    private readonly INamedPipe _pipe;

    internal NotificationHelper(string pipeName)
    {
        _pipe = new NamedPipeClient(pipeName);
        _pipe.Connect();
    }

    /// <summary>
    /// Notify the injecting process when injection has completed successfully
    /// and the plugin is about to be executed.
    /// </summary>
    /// <param name="processId">The process ID to send in the notification message.</param>
    /// <returns>True if the injection completion notification was sent successfully.</returns>
    internal bool SendInjectionComplete(int processId)
    {
        return _pipe.MessageHandler.TryWrite(BinaryInjection.IPC.InjectionCompleteNotification.CreateMessage(processId, true));
    }

    internal bool Log(string message, LogLevel level = LogLevel.Info)
    {
        return _pipe.MessageHandler.TryWrite(LogMessageNotification.CreateMessage(level, message));
    }

    public void Dispose()
    {
        _pipe?.Dispose();
    }
}
