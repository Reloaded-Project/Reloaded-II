namespace Reloaded.Mod.Loader.Logging;

public class Console
{
    /// <summary>
    /// Executed when the console is about to be closed.
    /// </summary>
    public event Action OnConsoleClose = () => { };

    /// <summary>
    /// Indicates if console is ready to be used or not.
    /// If console is not yet ready, all write operations will be buffered into the async collection.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// True if the console is enabled, else false.
    /// </summary>
    public bool IsEnabled { get; private set; }

    private Kernel32.ConsoleCtrlDelegate _consoleCtrlDelegate;
    private List<LogMessage> _messages = new List<LogMessage>();
    private Logger _logger;
    private IConsoleProxy _consoleProxy;

    /* Default constructor */

    /// <summary>
    /// Creates a new console instance, asynchronously initializing the console.
    /// </summary>
    /// <param name="enabled">True if console is enabled, else false.</param>
    /// <param name="logger">The logger associated with the console.</param>
    /// <param name="proxy">Proxy to the system console behind the scenes.</param>
    public Console(bool enabled, Logger logger, IConsoleProxy proxy)
    {
        IsEnabled = enabled;
        if (!IsEnabled)
            return;

        _consoleProxy = proxy;
        _logger = logger;
        _logger.WaitForConsoleInitFunc = WaitForConsoleInit;
        _logger.OnWriteLine += OnWriteLine;
        _logger.OnWrite     += OnWrite;

        Task.Run(() =>
        {
            var consoleAllocated = ConsoleAllocator.Alloc();
            if (!consoleAllocated)
                return;

            _consoleCtrlDelegate = NotifyOnConsoleClose;
            Kernel32.SetConsoleCtrlHandler(_consoleCtrlDelegate, true);
            _consoleProxy.SetBackColor(_logger.BackgroundColor);
            _consoleProxy.SetForeColor(_logger.TextColor);
            _consoleProxy.Clear();
            ReloadedBannerLogger.PrintBanner(proxy, logger);
            IsReady = true;

            FlushQueuedMessages();
        });
    }

    private void OnWrite(object sender, (string text, Color color) tuple)
    {
        if (!IsReady)
            _messages.Add(new LogMessage(LogMessageType.Write, tuple.text, tuple.color));
        else
            _consoleProxy.Write(tuple.text, tuple.color);
    }

    private void OnWriteLine(object sender, (string text, Color color) tuple)
    {
        if (!IsReady)
            _messages.Add(new LogMessage(LogMessageType.WriteLine, tuple.text, tuple.color));
        else
            _consoleProxy.WriteLine(tuple.text, tuple.color);
    }

    /// <summary>
    /// Synchronously waits for console initialization using blocking.
    /// </summary>
    public void WaitForConsoleInit(CancellationToken token = default)
    {
        if (!IsEnabled)
            return;

        while (!IsReady)
        {
            Thread.Sleep(1);
            if (token.IsCancellationRequested)
                return;
        }
    }

    private bool NotifyOnConsoleClose(Kernel32.CtrlTypes ctrltype)
    {
        if (ctrltype == Kernel32.CtrlTypes.CTRL_CLOSE_EVENT)
            OnConsoleClose?.Invoke();

        return false;
    }

    private void FlushQueuedMessages()
    {
        for (int x = 0; x < _messages.Count; x++)
        {
            var message = _messages[x];
            switch (message.Type)
            {
                case LogMessageType.WriteLine:
                    _consoleProxy.WriteLine(message.Message, message.Color);
                    break;
                case LogMessageType.Write:
                    _consoleProxy.Write(message.Message, message.Color);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _messages.Clear();
    }
}