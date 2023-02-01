namespace Reloaded.Mod.Loader.Logging;

public class Logger : ILogger
{
    /// <inheritdoc />
    public event EventHandler<string> OnPrintMessage;
        
    /// <inheritdoc />
    public event EventHandler<(string text, Color color)> OnWriteLine;

    /// <inheritdoc />
    public event EventHandler<(string text, Color color)> OnWrite;

    public Action<CancellationToken> WaitForConsoleInitFunc;

    private BlockingCollection<LogMessage> _messages = new BlockingCollection<LogMessage>();
    private Thread _loggingThread;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    public Logger()
    {
        _loggingThread = new Thread(ProcessQueue) { IsBackground = true };
        _loggingThread.Start();
    }

    /// <summary>
    /// Blocks the console from accepting any more asynchronous/buffered messages and
    /// waits until all existing buffered messages have been processed.
    /// </summary>
    public void Shutdown()
    {
        _cancellationToken.Cancel();
        while (_loggingThread.IsAlive)
            Thread.Sleep(1);
    }

    // Interface Implementation
    public void PrintMessage(string message, Color color)   => WriteLine(message, color);
    public void WriteLine(string message)                   => WriteLine(message, TextColor);
    public void Write(string message)                       => Write(message, TextColor);
    public void WriteLine(string message, Color color)
    {
        OnPrintMessage?.Invoke(this, message);
        OnWriteLine?.Invoke(this, (message, color));
    }

    public void Write(string message, Color color)
    {
        OnPrintMessage?.Invoke(this, message);
        OnWrite?.Invoke(this, (message, color));
    }

    public void WriteAsync(string message) => WriteAsync(message, TextColor);
    public void WriteLineAsync(string message) => WriteLineAsync(message, TextColor);
    public void WriteLineAsync(string message, Color color)
    {
        if (!_cancellationToken.IsCancellationRequested)
            _messages.Add(new LogMessage(LogMessageType.WriteLine, message, color));
    }

    public void WriteAsync(string message, Color color)
    {
        if (!_cancellationToken.IsCancellationRequested)
            _messages.Add(new LogMessage(LogMessageType.Write, message, color));
    }

    public void WaitForConsoleInit(CancellationToken token) => WaitForConsoleInitFunc?.Invoke(token);

    // Default Colours
    public Color BackgroundColor     { get; set; } = Color.FromArgb(20, 25, 31);
    public Color TextColor           { get; set; } = Color.FromArgb(239, 240, 235);

    public Color ColorRed            { get; set; } = Color.FromArgb(255, 92, 87);
    public Color ColorRedLight       { get; set; } = Color.FromArgb(220, 163, 163);

    public Color ColorGreen          { get; set; } = Color.FromArgb(90, 247, 142);
    public Color ColorGreenLight     { get; set; } = Color.FromArgb(195, 191, 159);

    public Color ColorYellow         { get; set; } = Color.FromArgb(243, 249, 157);
    public Color ColorYellowLight    { get; set; } = Color.FromArgb(240, 223, 175);

    public Color ColorBlue           { get; set; } = Color.FromArgb(87, 199, 255);
    public Color ColorBlueLight      { get; set; } = Color.FromArgb(148, 191, 243);

    public Color ColorPink           { get; set; } = Color.FromArgb(255, 106, 193);
    public Color ColorPinkLight      { get; set; } = Color.FromArgb(236, 147, 211);

    public Color ColorLightBlue      { get; set; } = Color.FromArgb(154, 237, 254);
    public Color ColorLightBlueLight { get; set; } = Color.FromArgb(147, 224, 227);

    public Color ColorInformation => ColorBlueLight;
    public Color ColorWarning => ColorYellowLight;
    public Color ColorError => ColorRedLight;
    public Color ColorSuccess => ColorGreenLight;

    // Business Logic
    private void ProcessQueue()
    {
        try
        {
            while (true)
            {
                var message = _messages.Take(_cancellationToken.Token);

                switch (message.Type)
                {
                    default:
                    case LogMessageType.WriteLine:
                        WriteLine(message.Message, message.Color);
                        break;
                    case LogMessageType.Write:
                        Write(message.Message, message.Color);
                        break;
                }

                // Exit thread if console is shutting down and we're done here.
                if (_messages.Count == 0 && _cancellationToken.IsCancellationRequested)
                    return;
            }
        }
        catch (OperationCanceledException)
        {
            // Process is terminating.
        }
    }
}