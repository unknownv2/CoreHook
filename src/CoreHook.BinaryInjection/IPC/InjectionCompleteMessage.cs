using System;

using CoreHook.IPC.Messages;

namespace CoreHook.BinaryInjection.IPC;

/// <summary>
/// A message containing information about an attempt to load
/// a plugin in a remote process. The message is sent from the
/// target application back to the host application that awaits it.
/// If the host application does not receive the message, then we assume
/// that the plugin loading has failed.
/// </summary>
public class InjectionCompleteMessage : CustomMessage
{
    /// <summary>
    /// True if the plugin load completed successfully, otherwise false.
    /// </summary>
    public bool Completed { get; }

    /// <summary>
    /// The process ID of the remote process in which the plugin has been loaded to.
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Initialize the plugin loading status message.
    /// </summary>
    /// <param name="didComplete">The status of the plugin load attempt, true if successful.</param>
    /// <param name="processId">The process ID of the process sending the message.</param>
    public InjectionCompleteMessage(bool didComplete, int processId)
    {
        Completed = didComplete;
        ProcessId = processId;
    }

    internal static InjectionCompleteMessage FromBody(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return null;
        }

        string[] dataParts = body.Split(MessageSeparator);

        if (dataParts.Length < 2)
        {
            throw new InvalidOperationException($"Invalid complete message. Expected at least 2 parts, got: {dataParts.Length} from message: '{body}'");
        }

        if (!int.TryParse(dataParts[0], out var processId))
        {
            throw new InvalidOperationException($"Invalid complete message. Expected PID, got: {dataParts[0]} from message: '{body}'");
        }

        if (!bool.TryParse(dataParts[1], out var didComplete))
        {
            throw new InvalidOperationException($"Invalid complete message. Expected bool for didComplete, got: {dataParts[1]} from message: '{body}'");
        }

        return new InjectionCompleteMessage(didComplete, processId);
    }

    /// <summary>
    /// Format the message information to a string.
    /// </summary>
    /// <returns>The message information.</returns>
    public override string ToMessage()
    {
        return string.Join(MessageSeparator.ToString(), ProcessId, Completed);
    }
}
