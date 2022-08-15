namespace Reloaded.AutoIndexBuilder.Validation;

internal class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.DiscordToken).NotNull().NotEmpty();
        RuleFor(x => x.GitPassword).NotNull().NotEmpty();
        RuleFor(x => x.GitUserName).NotNull().NotEmpty();
        RuleFor(x => x.GitRepoPath).NotNull().NotEmpty();
    }
}