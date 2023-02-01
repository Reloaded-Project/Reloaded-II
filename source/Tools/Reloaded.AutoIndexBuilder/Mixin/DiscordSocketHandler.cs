namespace Reloaded.AutoIndexBuilder.Mixin;

/// <summary>
/// Handles all logic related to the processing of Discord sockets.
/// </summary>
internal class DiscordSocketAdapter
{
    private readonly DiscordSocketClient _client;
    private readonly Settings _settings;
    private readonly Logger _logger;
    private readonly IServiceProvider _provider;
    private readonly IMediator _mediator;
    private readonly InteractionService _interactionService;

    public DiscordSocketAdapter(DiscordSocketClient client, Settings settings, Logger logger, IServiceProvider provider, IMediator mediator)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
        _provider = provider;
        _mediator = mediator;
        _interactionService = new InteractionService(_client, new InteractionServiceConfig()
        {
            DefaultRunMode = RunMode.Async,
            UseCompiledLambda = true
        });
        _client.Ready += OnDiscordReady;
        _client.Log += ClientOnLog;
    }

    /// <summary>
    /// Asynchronously initializes the instance.
    /// </summary>
    public async Task InitAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _settings.DiscordToken, true);
        await _client.StartAsync();
    }

    private async Task OnDiscordReady()
    {
        // SetupCommands
        await _interactionService.AddModulesAsync(typeof(DiscordSocketAdapter).Assembly, _provider);
        await _interactionService.RegisterCommandsGloballyAsync();
        _client.InteractionCreated += async (x) =>
        {
            var ctx = new SocketInteractionContext(_client, x);
            await _interactionService.ExecuteCommandAsync(ctx, _provider);
        };

        await _mediator.Publish(new DiscordReadyNotification());
    }

    private Task ClientOnLog(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
                _logger.Fatal("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
            case LogSeverity.Error:
                _logger.Error("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
            case LogSeverity.Warning:
                _logger.Warning("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
            case LogSeverity.Info:
                _logger.Information("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
            case LogSeverity.Verbose:
                _logger.Verbose("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
            case LogSeverity.Debug:
                _logger.Debug("Discord {0}, Src: {2}, Ex: {1}", message.Message, message.Exception, message.Source);
                break;
        }

        return Task.CompletedTask;
    }
}