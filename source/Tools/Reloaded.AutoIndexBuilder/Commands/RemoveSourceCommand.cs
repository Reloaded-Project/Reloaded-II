namespace Reloaded.AutoIndexBuilder.Commands;

public class RemoveSourceCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Settings _settings;
    private readonly IndexBuilderService _indexBuilder;

    public RemoveSourceCommand(Settings settings, IndexBuilderService indexBuilder)
    {
        _settings = settings;
        _indexBuilder = indexBuilder;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("remove", "Removes a specified source from the index.", false, RunMode.Async)]
    public async Task RemoveSource([Autocomplete(typeof(RemoveAutocompleteHandler))] string? sourceName)
    {
        var source = _settings.Sources.FirstOrDefault(x => x.FriendlyName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        if (source == null)
        {
            await RespondAsync(embed: Extensions.MakeErrorEmbed($"Source {sourceName} was not found!", "Source Not Found!"));
            return;
        }

        await _indexBuilder.RemoveSourceAsync(source);
        _settings.Sources.Remove(source);
        await _settings.SaveAsync();
        await RespondAsync(embed: Extensions.MakeSuccessEmbed($"Source {sourceName} has been removed!", "Source Removed!"));
    }

    public class RemoveAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var settings = services.GetRequiredService<Settings>();
            var test = autocompleteInteraction.Data.Current.Value.ToString();
            
            if (string.IsNullOrEmpty(test))
            {
                var results = settings.Sources.Select(x => new AutocompleteResult(x.FriendlyName, x.FriendlyName));
                return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25)));
            }
            else
            {
                // Note: This could be optimised.
                var results = settings.Sources.Where(x => x.FriendlyName.Contains(test, StringComparison.OrdinalIgnoreCase))
                                      .OrderBy(entry => entry.FriendlyName)
                                      .Select(x => new AutocompleteResult(x.FriendlyName, x.FriendlyName));

                return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25)));
            }
        }
    }
}