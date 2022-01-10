using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Reloaded.Mod.Loader.Tests.Update.Mocks;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.Providers;

public class AggregatePackageProviderTests
{
    [Fact]
    public async Task SearchAsync_ReturnsFromAllSources()
    {
        // Arrange
        var mockA = new Mock<IDownloadablePackageProvider>();
        mockA.Setup(x => x.SearchAsync("", 0, 50, default)).ReturnsAsync(() => new List<IDownloadablePackage>()
        {
            new DummyDownloadablePackage()
            {
                Id = "0"
            },
            new DummyDownloadablePackage()
            {
                Id = "1"
            },
        });

        var mockB = new Mock<IDownloadablePackageProvider>();
        mockB.Setup(x => x.SearchAsync("", 0, 50, default)).ReturnsAsync(() => new List<IDownloadablePackage>()
        {
            new DummyDownloadablePackage()
            {
                Id = "2"
            }
        });

        // Act
        var resolver = new AggregatePackageProvider(new[]
        {
            mockA.Object,
            mockB.Object
        });

        var result = await resolver.SearchAsync("", 0, 50, default);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, package => package.Id == "0");
        Assert.Contains(result, package => package.Id == "1");
        Assert.Contains(result, package => package.Id == "2");
    }
}