using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Reloaded.Mod.Loader.Update.Utilities;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet
{
    public class NugetHelperTests
    {
        private const string TestNugetFeed              = "http://167.71.128.50:5000/v3/index.json";
        private const string TestPackageName            = "sonicheroes.skins.midnighthill";
        private const string DependencyId               = "reloaded.universal.redirector";
        private const string TransitiveDependencyId     = "reloaded.sharedlib.hooks";

        private NugetHelper _nugetHelper;

        public NugetHelperTests()
        {
            _nugetHelper = NugetHelper.FromSourceUrlAsync(TestNugetFeed).Result;
        }

        [Fact]
        public void Search()
        {
            var package = _nugetHelper.Search(TestPackageName, false).Result;
            Assert.Contains(package, metadata => metadata.Identity.Id == TestPackageName);
        }

        [Fact]
        public void SearchDependencies()
        {
            var dependencies = _nugetHelper.FindDependencies(TestPackageName, false, true).Result;

            Assert.Contains(dependencies.Dependencies, metadata => metadata.Identity.Id == DependencyId);
            Assert.Contains(dependencies.Dependencies, metadata => metadata.Identity.Id == TransitiveDependencyId);
        }

        [Fact]
        public void DownloadPackage()
        {
            var downloadResourceResult = _nugetHelper.DownloadPackageAsync(TestPackageName, true, true).Result;
            var tempLocation           = Directory.CreateDirectory("Temp\\DownloadedMod");
            NugetHelper.ExtractPackageContent(downloadResourceResult, tempLocation.FullName);

            // Check ModConfig.json exists.
            string modConfigLocation = $"{tempLocation.FullName}\\ModConfig.json";
            Assert.True(File.Exists(modConfigLocation));

            Directory.Delete(tempLocation.FullName, true);
        }
    }
}
