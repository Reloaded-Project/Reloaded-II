using System.IO;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet
{
    public class NugetRepositoryTests
    {
        private const string TestNugetFeed              = "http://packages.sewer56.moe:5000/v3/index.json";
        private const string TestPackageName            = "sonicheroes.skins.midnighthill";
        private const string DependencyId               = "reloaded.universal.redirector";
        private const string TransitiveDependencyId     = "reloaded.sharedlib.hooks";

        private NugetRepository _nugetRepository;

        public NugetRepositoryTests()
        {
            _nugetRepository = NugetRepository.FromSourceUrl(TestNugetFeed);
        }

        [Fact]
        public void Search()
        {
            var package = _nugetRepository.Search(TestPackageName, false).Result;
            Assert.Contains(package, metadata => metadata.Identity.Id == TestPackageName);
        }

        [Fact]
        public void SearchDependencies()
        {
            var dependencies = _nugetRepository.FindDependencies(TestPackageName, false, true).Result;

            Assert.Contains(dependencies.Dependencies, metadata => metadata.Identity.Id == DependencyId);
            Assert.Contains(dependencies.Dependencies, metadata => metadata.Identity.Id == TransitiveDependencyId);
        }

        [Fact]
        public void DownloadPackage()
        {
            var downloadResourceResult = _nugetRepository.DownloadPackageAsync(TestPackageName, true, true).Result;
            var tempLocation           = Directory.CreateDirectory("Temp\\DownloadedMod");
            Nuget.ExtractPackage(downloadResourceResult, tempLocation.FullName);

            // Check ModConfig.json exists.
            string modConfigLocation = $"{tempLocation.FullName}\\ModConfig.json";
            Assert.True(File.Exists(modConfigLocation));

            Directory.Delete(tempLocation.FullName, true);
        }
    }
}
