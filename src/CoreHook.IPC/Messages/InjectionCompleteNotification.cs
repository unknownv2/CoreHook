
namespace CoreHook.IPC.Messages;

/// <summary>
/// Notification message sent to the host process
/// about the success of a module loading attempt in a target process.
/// </summary>
public class InjectionCompleteNotification
{
    /// <summary>
    /// The message type.
    /// </summary>
    public const string InjectionComplete = "InjectionComplete";

    /// <summary>
    /// The message data.
    /// </summary>
    private readonly InjectionCompleteMessage _requestData;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectionCompleteNotification"/> class.
    /// </summary>
    /// <param name="body">The message data.</param>
    public InjectionCompleteNotification(string body)
    {
        _requestData = InjectionCompleteMessage.FromBody(body);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectionCompleteNotification"/> class.
    /// </summary>
    /// <param name="processId">The target process ID.</param>
    /// <param name="didComplete">True if the module loading attempt completed successfully.</param>
    public InjectionCompleteNotification(int processId, bool didComplete)
    {
        _requestData = new InjectionCompleteMessage(didComplete, processId);
    }

    /// <summary>
    /// Format the message data to a <see cref="IStringMessage"/>.
    /// </summary>
    /// <returns>The formatted message data.</returns>
    public IStringMessage CreateMessage()
    {
        return new StringMessage(InjectionComplete, _requestData.ToMessage());
    }

    /// <summary>
    /// Format new message information into a <see cref="IStringMessage"/>.
    /// </summary>
    /// <param name="processId">The target process ID.</param>
    /// <param name="didComplete">True if the module loading attempt completed successfully.</param>
    /// <returns>The formatted message data.</returns>
    public static IStringMessage CreateMessage(int processId, bool didComplete)
    {
        return new InjectionCompleteNotification(processId, didComplete).CreateMessage();
    }

    /// <summary>
    /// Construct a <see cref="InjectionCompleteMessage"/> from string message data.
    /// </summary>
    /// <param name="message">The serialized message data.</param>
    /// <returns>The newly constructed message data container.</returns>
    public static InjectionCompleteMessage ParseMessage(IStringMessage message)
    {
        return InjectionCompleteMessage.FromBody(message.Body);
    }
}
