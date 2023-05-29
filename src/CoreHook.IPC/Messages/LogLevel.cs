
namespace CoreHook.IPC.Messages;

/// <summary>
/// Defines the types of message information.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Debug diagnostic message.
    /// </summary>
    Debug = 0,
    /// <summary>
    /// Release diagnostic message.
    /// </summary>
    Trace,
    /// <summary>
    /// General information message.
    /// </summary>
    Info,
    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,
    /// <summary>
    /// Error message.
    /// </summary>
    Error
}
