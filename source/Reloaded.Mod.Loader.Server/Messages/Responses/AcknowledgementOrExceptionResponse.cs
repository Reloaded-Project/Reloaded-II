namespace Reloaded.Mod.Loader.Server.Messages.Responses;

/// <summary>
/// Represents an acknowledgement for messages without a return value or an exception
/// while processing the request. 
/// </summary>
public struct AcknowledgementOrExceptionResponse : IMessage<AcknowledgementOrExceptionResponse, MessagePackSerializer<AcknowledgementOrExceptionResponse>, BrotliCompressor>, IKeyedMessage, IPackable
{
    public sbyte GetMessageType() => (sbyte)MessageType.Acknowledgement;
    public MessagePackSerializer<AcknowledgementOrExceptionResponse> GetSerializer() => new();
    public BrotliCompressor GetCompressor() => new();

    /// <summary>
    /// The exception message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The stack trace for the exception;
    /// </summary>
    public string? StackTrace { get; set; }

    /// <inheritdoc/>
    public MessageKey Key { get; set; }

    /// <summary>
    /// Creates an exception server side.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="stackTrace">Optional stack trace to include with the exception.</param>
    /// <param name="key">Individual key associated with the original message.</param>
    public AcknowledgementOrExceptionResponse(string? message, string? stackTrace, MessageKey key = default)
    {
        Message = message;
        StackTrace = stackTrace;
        Key = key;
    }

    /// <summary>
    /// Creates an acknowledgement server side.
    /// </summary>
    /// <param name="key">Individual key associated with the original message.</param>
    public AcknowledgementOrExceptionResponse(MessageKey key = default)
    {
        Key = key;
        Message = null;
        StackTrace = null;
    }

    /// <summary>
    /// True if this message represents an exception, else false.
    /// </summary>
    public bool IsException() => Message != null;

    /// <inheritdoc/>
    public ReusableSingletonMemoryStream Pack() => this.Serialize(ref this);
}