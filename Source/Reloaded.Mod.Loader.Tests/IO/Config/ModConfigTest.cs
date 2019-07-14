using System;
using System.Collections.Generic;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.SETUP;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO.Config
{
    public class ModConfigTest : IDisposable
    {
        /* Setup Loader Config */
        public TestData TestData { get; set; } = new TestData();

        public void Dispose()
        {
            TestData?.Dispose();
        }

        [Fact]
        public void GetNestedMissingDependencies()
        {
            var dependencies = ModConfig.GetDependencies(TestData.TestModConfigC);

            foreach (var missingDependency in TestData.NonexistingDependencies)
                Assert.Contains(missingDependency, dependencies.MissingConfigurations);
        }

        [Fact]
        public void GetNestedDependencies()
        {
            var dependencies = ModConfig.GetDependencies(TestData.TestModConfigC);

            Assert.Contains(TestData.TestModConfigA, dependencies.Configurations);
            Assert.Contains(TestData.TestModConfigB, dependencies.Configurations);
        }

        [Fact]
        public void SortMods()
        {
            var dependencies  = ModConfig.GetDependencies(TestData.TestModConfigC);
            var allMods       = new List<ModConfig>();

            allMods.Add(TestData.TestModConfigC);
            allMods.AddRange(dependencies.Configurations);
            allMods = ModConfig.SortMods(allMods);

            Assert.Equal(TestData.TestModConfigA, allMods[0]);
            Assert.Equal(TestData.TestModConfigB, allMods[1]);
            Assert.Equal(TestData.TestModConfigC, allMods[2]);
        }
    }
}
