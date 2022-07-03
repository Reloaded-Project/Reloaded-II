namespace Reloaded.Mod.Loader.Logging.Structs;

internal struct LogMessage
{
    public LogMessageType Type { get; set; }
    public string Message { get; set; }
    public Color Color    { get; set; }

    public LogMessage(LogMessageType type, string message, Color color)
    {
        Type = type;
        Message = message;
        Color = color;
    }
}

internal enum LogMessageType : byte
{
    WriteLine,
    Write
}