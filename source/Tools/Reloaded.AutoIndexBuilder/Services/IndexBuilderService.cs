namespace Reloaded.AutoIndexBuilder.Services;

/// <summary>
/// Service that maintains a queue of items to build, and builds them, reporting back.
/// </summary>
public class IndexBuilderService : IJob
{
    private const string JobSourceEntryKey = "source";

    private static SemaphoreSlim _blockConcurrencySema = new SemaphoreSlim(1);
    private static IndexBuilderService _instance = null!;
    private readonly Settings _settings;
    private readonly Logger _logger;
    private readonly Stats _stats;
    private readonly GitPusherService _gitPusherService;
    private readonly IMediator _mediator;
    private readonly IScheduler _scheduler;
    private readonly IServiceProvider _diContainer;

    // For quartz.
#pragma warning disable CS8618
    public IndexBuilderService() { }
#pragma warning restore CS8618

    public IndexBuilderService(Settings settings, Logger logger, Stats stats, GitPusherService gitPusherService, IMediator mediator, ISchedulerFactory schedulerFactory, IServiceProvider diContainer)
    {
        _instance = this;
        _settings = settings;
        _logger = logger;
        _stats = stats;
        _gitPusherService = gitPusherService;
        _mediator = mediator;
        _diContainer = diContainer;
        _scheduler = Task.Run(() => schedulerFactory.GetScheduler()).GetAwaiter().GetResult();
        _scheduler.Start();
    }

    /// <summary>
    /// Causes all scheduled tasks to run immediately.
    /// </summary>
    public async Task ForceRunAsync()
    {
        var keys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        foreach (var key in keys)
            await _scheduler.TriggerJob(key);
    }

    /// <summary>
    /// Called after Discord is ready.
    /// </summary>
    public async Task InitAsync()
    {
        foreach (var source in _settings.Sources)
        {
            if (!source.Enabled)
                continue;

            await ScheduleSource(source);
        }
    }

    /// <summary>
    /// Adds a source to the schedule, runs it immediately.
    /// </summary>
    public async Task ScheduleSource(SourceEntry source)
    {
        var jobData = new JobDataMap();
        jobData[JobSourceEntryKey] = source;

        var job = JobBuilder.Create<IndexBuilderService>()
            .WithIdentity(source.FriendlyName)
            .UsingJobData(jobData)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(source.FriendlyName)
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(source.MinutesBetweenRefresh)
                .RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }

    /// <summary>
    /// Removes a source from the scheduler.
    /// </summary>
    public async Task RemoveSourceAsync(SourceEntry source)
    {
        await _scheduler.DeleteJob(JobKey.Create(source.FriendlyName));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _blockConcurrencySema.WaitAsync();
        try
        {
            await _instance.ExecuteInternal(context);
        }
        finally
        {
            _blockConcurrencySema.Release();
        }
    }

    private async Task ExecuteInternal(IJobExecutionContext context)
    {
        // Pull from Git
        _gitPusherService.ResetToRemote();

        // Build
        var watch = Stopwatch.StartNew();
        var source = (SourceEntry)context.MergedJobDataMap[JobSourceEntryKey];

        // Create Index
        var builder  = new IndexBuilder();
        builder.Sources.Add(source);

        // Update Index
        var indexApi = new IndexApi(_settings.GitRepoPath);
        var index    = await indexApi.GetOrCreateLocalIndexAsync();
        index = await builder.UpdateAsync(index);

        // Delete Unused
        var cleanupBuilder = new IndexBuilder();
        cleanupBuilder.Sources.AddRange(_settings.Sources);
        cleanupBuilder.RemoveNotInBuilder(index);
        await cleanupBuilder.WriteToDiskAsync(index);

        // Update Stats
        _stats.TotalBuilds += 1;
        _stats.BuildsSinceStarted += 1;
        await _stats.WriteAsync();

        // Notify others.
        await _mediator.Publish(new BuildFinishedNotification()
        {
            Entry = source,
            Runtime = watch.Elapsed,
        });

        // Push to git.
        _gitPusherService.Push();
    }
}