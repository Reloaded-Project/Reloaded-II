namespace Reloaded.AutoIndexBuilder.Commands;

public class AddSourceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Logger _logger;
    private readonly Settings _settings;
    private readonly IndexBuilderService _indexBuilder;

    public AddSourceCommand(Logger logger, Settings settings, IndexBuilderService indexBuilder)
    {
        _logger = logger;
        _settings = settings;
        _indexBuilder = indexBuilder;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("addnuget", "Adds a NuGet source to the bot.", false, RunMode.Default)]
    public async Task AddNuGet(string? nugetUrl, string friendlyName, int? durationMinutes = 45)
    {
        // Validate
        if (string.IsNullOrEmpty(nugetUrl))
        {
            await RespondAsync(embed: Extensions.MakeErrorEmbed("You must specify a NuGet URL"));
            return;
        }
        
        try
        {
            var provider = new NuGetPackageProvider(NugetRepository.FromSourceUrl(nugetUrl));
            var results = await provider.SearchAsync("", 0, 10);
            await AddSourceCommon(new SourceEntry(nugetUrl), friendlyName, durationMinutes!.Value);
        }
        catch (Exception e)
        {
            await HandleSourceError(e);
        }
    }
    
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("addgamebanana", "Adds a NuGet source to the bot.", false, RunMode.Default)]
    public async Task AddGameBanana(int? appId, string friendlyName, int? durationMinutes = 45)
    {
        // Validate
        if (!appId.HasValue)
        {
            await RespondAsync(embed: Extensions.MakeErrorEmbed("You must specify a GameBanana app id."));
            return;
        }

        try
        {
            var provider = new GameBananaPackageProvider(appId.Value);
            var results = await provider.SearchAsync("", 0, 10);
            await AddSourceCommon(new SourceEntry(appId.Value), friendlyName, durationMinutes!.Value);
        }
        catch (Exception e)
        {
            await HandleSourceError(e);
        }
    }

    private async Task HandleSourceError(Exception e)
    {
        await RespondAsync(embed: Extensions.MakeErrorEmbed($"Failed to add source, maybe it didn't respond?\n" +
                                                            $"{e.Message}\n" +
                                                            $"{e.StackTrace}"));
    }

    private async Task AddSourceCommon(SourceEntry source, string friendlyName, int durationMinutes)
    {
        source.Enabled = true;
        source.FriendlyName = friendlyName;
        source.MinutesBetweenRefresh = durationMinutes;
        _settings.Sources.Add(source);
        await _settings.SaveAsync();
        await _indexBuilder.ScheduleSource(source);
        await RespondAsync(embed: Extensions.MakeSuccessEmbed($"Successfully added source!"));
        _logger.Information("Added a Source for building: {@source}", source);
    }
}