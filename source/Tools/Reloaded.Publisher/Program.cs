namespace Reloaded.Publisher;

internal class Program
{
    static async Task Main(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.AutoHelp = true;
            with.CaseSensitive = false;
            with.CaseInsensitiveEnumValues = true;
            with.EnableDashDash = true;
            with.HelpWriter = null;
        });

        // Fix for single file publish issue.
        ConfigurationManager.AppSettings["7zLocation"] = Path.Combine(AppContext.BaseDirectory, Environment.Is64BitProcess ? "7z64.dll" : "7z.dll");
        var parserResult = parser.ParseArguments<PublishModOptions>(args);
        await parserResult.WithParsedAsync<PublishModOptions>(PublishModAsync);
        parserResult.WithNotParsed(errs => HandleParseError(parserResult, errs));
    }

    private static async Task PublishModAsync(PublishModOptions options)
    {
        using var progressBar = new ShellProgressBar.ProgressBar(10000, "Building Release");
        var configPath = Path.Combine(options.ModFolder, ModConfig.ConfigFileName);
        var config     = await IConfig<ModConfig>.FromPathAsync(configPath);
        await Mod.Loader.Update.Packaging.Publisher.PublishAsync(new Mod.Loader.Update.Packaging.Publisher.PublishArgs()
        {
            ModTuple = new PathTuple<ModConfig>(configPath, config),
            OutputFolder = options.OutputFolder,
            IncludeRegexes = options.IncludeRegexes.ToList(),
            IgnoreRegexes = options.IgnoreRegexes.ToList(),
            OlderVersionFolders = options.OlderVersionFolders.ToList(),
            AutomaticDelta = options.AutomaticDelta,
            CompressionLevel = options.CompressionLevel,
            CompressionMethod = options.CompressionMethod,
            Progress = progressBar.AsProgress<double>(),
            PackageName = options.PackageName,
            PublishTarget = options.PublishTarget,
            ChangelogPath = options.ChangelogPath,
            ReadmePath = options.ReadmePath,
            MetadataFileName = config.ReleaseMetadataFileName
        });
    }

    /// <summary>
    /// Errors or --help or --version.
    /// </summary>
    static void HandleParseError(ParserResult<PublishModOptions> options, IEnumerable<Error> errs)
    {
        var helpText = HelpText.AutoBuild(options, help =>
        {
            help.Copyright = "Created by Sewer56, licensed under GNU LGPL V3";
            help.AutoHelp = false;
            help.AutoVersion = false;
            help.AddDashesToOption = true;
            help.AddEnumValuesToHelpText = true;
            help.AddNewLineBetweenHelpSections = true;
            help.AdditionalNewLineAfterOption = true;
            return HelpText.DefaultParsingErrorsHandler(options, help);
        }, example => example, true);

        Console.WriteLine(helpText);
    }
}