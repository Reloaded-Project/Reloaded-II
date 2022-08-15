namespace Reloaded.AutoIndexBuilder;

internal class Program
{
    private static IServiceProvider _diProvider = null!;

    static async Task Main(string[] args)
    {
        // Try read config.
        var config = Settings.TryRead();
        if (config == null)
            return;
        
        // Setup DI Container.
        _diProvider = ConfigureDependencyInjection(config);

        // Start the index builder.
        _diProvider.GetRequiredService<IndexBuilderService>();

        // Setup Discord
        var discord = _diProvider.GetRequiredService<DiscordSocketAdapter>();
        await discord.InitAsync();

        // Go go go!
        await Task.Delay(-1);
    }
    
    private static IServiceProvider ConfigureDependencyInjection(Settings settings)
    {
        var collection = new ServiceCollection();
        collection.AddSingleton(new DiscordSocketConfig());
        collection.AddSingleton<DiscordSocketClient>();
        collection.AddSingleton<DiscordSocketAdapter>();
        collection.AddSingleton<GitPusherService>();
        collection.AddSingleton<Settings>(settings);
        collection.AddSingleton<Stats>(Stats.Get());
        collection.AddTransient<DiscordErrorLoggerSink>();

        collection.AddLogging(x => x.AddSerilog());
        collection.AddSingleton<Logger>(sp =>
        {
            var loggerConf = new LoggerConfiguration()
                .WriteTo.Sink(sp.GetRequiredService<DiscordErrorLoggerSink>())
                .WriteTo.File($"log-warning-error.txt", rollingInterval: RollingInterval.Year, restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.File($"log-all.txt", rollingInterval: RollingInterval.Month);

#if DEBUG
            loggerConf = loggerConf.WriteTo.Console();
#endif

            var logger = loggerConf.CreateLogger();
            Log.Logger = logger;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            return logger;
        });

        collection.AddTransient<IndexBuilderService>();
        collection.AddQuartz(configurator =>
        {
            configurator.UseInMemoryStore();
            configurator.SchedulerName = "Cool Scheduler";
        });
        
        collection.AddMediatR(configuration => { configuration.AsSingleton(); }, typeof(Program));
        return collection.BuildServiceProvider();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal("{@e}", e);
        if (e.IsTerminating)
            Log.CloseAndFlush();
    }
}