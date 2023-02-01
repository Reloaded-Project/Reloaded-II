using System.Text.Json.Serialization;
using Reloaded.Community.Tool.Serialization;
using Index = Reloaded.Mod.Loader.Community.Config.Index;

namespace Reloaded.Community.Tool;

public class Program
{
    static void Main(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.AutoHelp = true;
            with.CaseSensitive = false;
            with.CaseInsensitiveEnumValues = true;
            with.EnableDashDash = true;
            with.HelpWriter = null;
        });
        
        var parserResult = parser.ParseArguments<BuildIndexOptions, PrintTemplateOptions, CreateTemplateOptions, HashOptions>(args);
        parserResult.WithParsed<BuildIndexOptions>(BuildIndex)
            .WithParsed<PrintTemplateOptions>(PrintTemplate)
            .WithParsed<CreateTemplateOptions>(CreateTemplate)
            .WithParsed<HashOptions>(Hash)
            .WithNotParsed(errs => HandleParseError(parserResult, errs));
    }

    private static void Hash(HashOptions obj)
    {
        using var fileStream = new FileStream(obj.Source, FileMode.Open);
        Console.WriteLine(Hashing.ToString(xxHash64.ComputeHash(fileStream)));
    }

    private static void BuildIndex(BuildIndexOptions buildIndex) => Index.Build(buildIndex.Source, buildIndex.Destination);

    private static void CreateTemplate(CreateTemplateOptions createTemplate)
    {
        if (createTemplate.Type == TemplateType.Application)
        {
            var apps   = ApplicationConfig.GetAllApplications();
            var appById = apps.FirstOrDefault(x => x.Config.AppId == createTemplate.Id);

            if (appById == null)
            {
                Console.WriteLine($"Possible Applications");
                foreach (var app in apps)
                    Console.WriteLine($"Id: {app.Config.AppId} | Name: {app.Config.AppName}");

                var cmdLine = Parser.Default.FormatCommandLine(new CreateTemplateOptions()
                {
                    Id = apps[0].Config.AppId,
                    Type = TemplateType.Application
                });

                Console.WriteLine($"Example Usage: `Reloaded.Community.Tool.exe {cmdLine}`");
            }
            else
            {
                Console.WriteLine("Copy Text Below, Save as .json file.\n" +
                                  "====================================");

                var config = TryGetGameBananaUpdateConfig(appById);
                SerializeAndPrint(new AppItem()
                {
                    AppId = appById.Config.AppId,
                    AppName = appById.Config.AppName,
                    AppStatus = Status.Ok,
                    Hash = Hashing.ToString(xxHash64.ComputeHash(File.ReadAllBytes(ApplicationConfig.GetAbsoluteAppLocation(appById)))),
                    GameBananaId = config.GameId
                });
            }
        }
        else
        {
            throw new Exception("Not Supported");
        }
    }

    private static void PrintTemplate(PrintTemplateOptions printTemplate)
    {
        switch (printTemplate.Type)
        {
            case TemplateType.Application:
                SerializeAndPrint(Samples.AppItem);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void SerializeAndPrint<T>(T item)
    {
        Console.WriteLine(JsonSerializer.Serialize(item, new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));
    }

    /// <summary>
    /// Errors or --help or --version.
    /// </summary>
    static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
    {
        var helpText = HelpText.AutoBuild(options, help =>
        {
            help.Copyright = "Created by Sewer56, licensed under GNU LGPL V3\n" +
                             "Please see: https://github.com/Reloaded-Project/Reloaded.Community for more details.";
            help.AutoHelp = false;
            help.AutoVersion = false;
            help.AddDashesToOption = true;
            help.AddEnumValuesToHelpText = true;
            help.AdditionalNewLineAfterOption = true;
            return HelpText.DefaultParsingErrorsHandler(options, help);
        }, example => example, true);

        Console.WriteLine(helpText);
    }

    private static GameBananaProviderConfig TryGetGameBananaUpdateConfig(PathTuple<ApplicationConfig> appById)
    {
        try
        {
            appById.Config.PluginData.TryGetValue("GBPackageProvider", out GameBananaProviderConfig config);
            config ??= new GameBananaProviderConfig();
            return config;
        }
        catch (Exception)
        {
            return new GameBananaProviderConfig();
        }
    }
}