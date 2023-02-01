using IOEx = Sewer56.DeltaPatchGenerator.Lib.Utility.IOEx;
using IPackageResolver = Sewer56.Update.Interfaces.IPackageResolver;

namespace Reloaded.Mod.Loader.Tests.Update;

public class UpdaterTests : IDisposable
{
    private const string TestArchiveFile = "HeroesControllerPostProcess.zip";
    private const string TestArchiveFileOld = "HeroesControllerPostProcessOld.zip";

    private TestEnvironmoent _testEnvironmoent;
    private TemporaryFolderAllocation _packageFolder;
    private TemporarySetNewUpdaterResolvers _temporaryResolvers;
    private LocalPackageUpdateResolverFactory _factory;

    private UpdaterData _updaterData = new(new List<string>(), new CommonPackageResolverSettings());
    private List<string> _foldersToDelete = new List<string>();

    public UpdaterTests()
    {
        _testEnvironmoent = new TestEnvironmoent();
        _packageFolder = new TemporaryFolderAllocation();
        _factory = new LocalPackageUpdateResolverFactory(_packageFolder.FolderPath);

        _temporaryResolvers = new TemporarySetNewUpdaterResolvers(new IUpdateResolverFactory[] { _factory });

        // Create Packages
        using var newPackageFolder = new TemporaryFolderAllocation();
        using var oldPackageFolder = new TemporaryFolderAllocation();
        var extractor = new ZipPackageExtractor();
        Task.Run(() => extractor.ExtractPackageAsync(TestArchiveFile, newPackageFolder.FolderPath)).GetAwaiter().GetResult();
        Task.Run(() => extractor.ExtractPackageAsync(TestArchiveFileOld, oldPackageFolder.FolderPath)).GetAwaiter().GetResult();

        var targetModDirectory = Path.Combine(_testEnvironmoent.TestConfig.GetModConfigDirectory(), "OldPackage");
        IOEx.CopyDirectory(oldPackageFolder.FolderPath, targetModDirectory);
        _foldersToDelete.Add(targetModDirectory);

        Task.Run(() => PublishAsync(newPackageFolder.FolderPath, oldPackageFolder.FolderPath, _packageFolder.FolderPath)).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _testEnvironmoent.Dispose();
        _packageFolder.Dispose();
        _temporaryResolvers.Dispose();

        foreach (var folderToDelete in _foldersToDelete)
            IOEx.TryDeleteDirectory(folderToDelete);
    }

    async Task PublishAsync(string modFolder, string oldModFolder, string outputFolder)
    {
        var configFilePath = Path.Combine(modFolder, ModConfig.ConfigFileName);
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException($"Failed to convert folder to NuGet Package. Unable to find config at {configFilePath}");

        var config = await IConfig<ModConfig>.FromPathAsync(configFilePath);
        var result = await Publisher.PublishAsync(new Publisher.PublishArgs()
        {
            PublishTarget = Publisher.PublishTarget.Default,
            ModTuple = new PathTuple<ModConfig>(configFilePath, config),
            OlderVersionFolders = new List<string>() { oldModFolder },
            OutputFolder = outputFolder
        });
    }

    [Fact]
    public async Task GetUpdateDetails_ReturnsAvailableUpdate()
    {
        var updater = new Updater(_testEnvironmoent.ModConfigService, _testEnvironmoent.UserConfigService, _updaterData);
        var details = await updater.GetUpdateDetailsAsync();
        Assert.Single(details.ManagerModResultPairs);
    }

    [Fact]
    public async Task GetUpdateDetails_CanGetUpdateInfo()
    {
        var updater = new Updater(_testEnvironmoent.ModConfigService, _testEnvironmoent.UserConfigService, _updaterData);
        var details = await updater.GetUpdateDetailsAsync();
        Assert.NotNull(details.GetUpdateInfo());
    }

    [Fact]
    public async Task GetUpdateDetails_CanUpdate()
    {
        var updater = new Updater(_testEnvironmoent.ModConfigService, _testEnvironmoent.UserConfigService, _updaterData);
        var details = await updater.GetUpdateDetailsAsync();
        var oldConfig  = details.ManagerModResultPairs[0].ModTuple;
        var oldVersion = oldConfig.Config.ModVersion;

        await updater.Update(details);

        var newConfig = await IConfig<ModConfig>.FromPathAsync(oldConfig.Path);
        Assert.True(NuGetVersion.Parse(newConfig.ModVersion) > NuGetVersion.Parse(oldVersion));
    }
}

[ExcludeFromCodeCoverage]
public class TemporarySetNewUpdaterResolvers : IDisposable
{
    public IUpdateResolverFactory[] OriginalFactories { get; private set; }

    public TemporarySetNewUpdaterResolvers(IUpdateResolverFactory[] originalFactories)
    {
        OriginalFactories = PackageResolverFactory.All;
        PackageResolverFactory.SetResolverFactories(originalFactories);
    }

    public void Dispose()
    {
        PackageResolverFactory.SetResolverFactories(OriginalFactories);
    }
}


[ExcludeFromCodeCoverage]
public class LocalPackageUpdateResolverFactory : IUpdateResolverFactory
{

    private static IPackageExtractor _extractor = new SevenZipSharpExtractor();

    /// <inheritdoc />
    public IPackageExtractor Extractor { get; } = _extractor;

    public string ResolverId { get; } = "LocalPackageResolver";
    public string FriendlyName { get; } = "LocalPackageResolver";
    public string Directory { get; }

    public LocalPackageUpdateResolverFactory(string directory) => Directory = directory;

    public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig) { }

    public IPackageResolver GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig> userConfig, UpdaterData data)
    {
        return new LocalPackageResolver(Directory);
    }

    public bool TryGetConfigurationOrDefault(PathTuple<ModConfig> mod, out object configuration)
    {
        configuration = default!;
        return false;
    }
}