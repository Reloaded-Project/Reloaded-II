namespace Reloaded.Mod.Loader.Tests.Update.NuGet;

public class AggregateNuGetRepositoryTests
{
    // Dependency in another source (Redirector) & Hooks. (Hooks is Transitive from Redirector) 
    public static readonly PackageIdentity Skin = new("sonicheroes.skins.midnighthill", NuGetVersion.Parse("1.0.0"));

    // Dependency in same source (Hooks)
    public static readonly PackageIdentity Redirector = new("reloaded.universal.redirector", NuGetVersion.Parse("2.0.0"));

    // Missing Dependency + In Another Source (Hooks)
    public static readonly PackageIdentity Missing = new("riders.tweakbox", NuGetVersion.Parse("4.0.0"));

    // No Dependencies
    public static readonly PackageIdentity Hooks = new("reloaded.sharedlib.hooks", NuGetVersion.Parse("1.3.0"));

    private const string MissingDependencyName = "fake.mod";

    public INugetRepository _dependenciesRepository;
    public INugetRepository _otherModsRepository;
    public AggregateNugetRepository _aggregateRepository;

    public AggregateNuGetRepositoryTests()
    {
        _dependenciesRepository = MakeRedirectorMock();
        _otherModsRepository = MakeOtherMock();
        _aggregateRepository = new AggregateNugetRepository(new[] { _dependenciesRepository, _otherModsRepository });
    }

    private INugetRepository MakeRedirectorMock()
    {
        var mock       = new Mock<INugetRepository>();
        var redirector = new CustomPackageSearchMetadata(Redirector);
        var hooks      = GetPackage(Hooks);

        redirector.DependencySets = new[] { new PackageDependencyGroup(new NuGetFramework("Reloaded"), new[] { new PackageDependency(Hooks.Id) }) };

        // Setup packages.
        mock.Setup(library => library.GetPackageDetails(Redirector.Id, false, false, It.IsAny<CancellationToken>())).ReturnsAsync(ToList(redirector));
        mock.Setup(library => library.GetPackageDetails(Hooks.Id, false, false, It.IsAny<CancellationToken>())).ReturnsAsync(ToList(hooks));

        return mock.Object;
    }

    private INugetRepository MakeOtherMock()
    {
        var mock = new Mock<INugetRepository>();

        var skin   = new CustomPackageSearchMetadata(Skin);
        var tweakbox = new CustomPackageSearchMetadata(Missing);
        skin.DependencySets = new []{ new PackageDependencyGroup(new NuGetFramework("Reloaded"), new []{ new PackageDependency(Redirector.Id) }) };
        tweakbox.DependencySets = new[] { new PackageDependencyGroup(new NuGetFramework("Reloaded"), new[] { new PackageDependency(Hooks.Id), new PackageDependency(MissingDependencyName) }) };

        // Setup packages.
        mock.Setup(library => library.GetPackageDetails(Skin.Id, false, false, It.IsAny<CancellationToken>())).ReturnsAsync(ToList(skin));
        mock.Setup(library => library.GetPackageDetails(Missing.Id, false, false, It.IsAny<CancellationToken>())).ReturnsAsync(ToList(tweakbox));

        return mock.Object;
    }

    private IPackageSearchMetadata GetPackage(PackageIdentity identity) => PackageSearchMetadataBuilder.FromIdentity(identity).Build();
    private List<IPackageSearchMetadata> ToList(params IPackageSearchMetadata[] metadata) => metadata.ToList();

    [Fact]
    public async void Mocks()
    {
        var skinAsync        = await _otherModsRepository.GetPackageDetails(Skin.Id, false, false);
        var missingAsync     = await _otherModsRepository.GetPackageDetails(Missing.Id, false, false);
        var redirectorAsync  = await _dependenciesRepository.GetPackageDetails(Redirector.Id, false, false);
        var hooksAsync       = await _dependenciesRepository.GetPackageDetails(Hooks.Id, false, false);

        var skin = _otherModsRepository.GetPackageDetails(Skin.Id, false, false).Result;
        var missing = _otherModsRepository.GetPackageDetails(Missing.Id, false, false).Result;
        var redirector = _dependenciesRepository.GetPackageDetails(Redirector.Id, false, false).Result;
        var hooks = _dependenciesRepository.GetPackageDetails(Hooks.Id, false, false).Result;

        Assert.NotEmpty(skinAsync);
        Assert.NotEmpty(missingAsync);
        Assert.NotEmpty(redirectorAsync);
        Assert.NotEmpty(hooksAsync);

        Assert.NotEmpty(skin);
        Assert.NotEmpty(missing);
        Assert.NotEmpty(redirector);
        Assert.NotEmpty(hooks);
    }

    [Fact]
    public void GetDetails()
    {
        var skinSearchResult = _aggregateRepository.GetPackageDetails(Skin.Id, false, false).Result;
        var hookSearchResult = _aggregateRepository.GetPackageDetails(Hooks.Id, false, false).Result;

        // First source
        Assert.Contains(hookSearchResult, source => source.Generic.Any(x => x.Identity.Id == Hooks.Id));
            
        // Second source
        Assert.Contains(skinSearchResult, source => source.Generic.Any(x => x.Identity.Id == Skin.Id));
    }

    [Fact]
    public void SearchDependencies()
    {
        var redirectorDeps = _aggregateRepository.FindDependencies(Redirector.Id, false, false).Result;
        var skinDeps = _aggregateRepository.FindDependencies(Skin.Id, false, false).Result;
        var noDeps = _aggregateRepository.FindDependencies(Hooks.Id, false, false).Result;
        var missingDeps = _aggregateRepository.FindDependencies(Missing.Id, false, false).Result;

        // No Dependencies
        Assert.Single(noDeps.Dependencies);
        Assert.Empty(noDeps.PackagesNotFound);

        // Same Source
        Assert.Contains(redirectorDeps.Dependencies, metadata => metadata.Generic.Identity.Id == Hooks.Id);
        Assert.Empty(redirectorDeps.PackagesNotFound);

        // Another Source
        Assert.Contains(skinDeps.Dependencies, metadata => metadata.Generic.Identity.Id == Redirector.Id);
        Assert.Contains(skinDeps.Dependencies, metadata => metadata.Generic.Identity.Id == Hooks.Id);
        Assert.Empty(skinDeps.PackagesNotFound);

        // Missing Deps
        Assert.Contains(missingDeps.Dependencies, metadata => metadata.Generic.Identity.Id == Hooks.Id);
        Assert.Contains(missingDeps.PackagesNotFound, notFound => notFound == MissingDependencyName);
    }
}