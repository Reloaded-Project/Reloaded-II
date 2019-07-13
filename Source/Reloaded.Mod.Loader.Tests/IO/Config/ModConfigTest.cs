using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.SETUP;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO.Config
{
    public class ModConfigTest
    {
        public ModConfigTestData TestData { get; set; } = new ModConfigTestData();

        [Fact]
        public void GetNestedMissingDependencies()
        {
            var dependencies = ModConfig.GetDependencies(TestData.TestModCConfig, TestData.AllMods);

            foreach (var missingDependency in TestData.NonexistingDependencies)
                Assert.Contains(missingDependency, dependencies.MissingConfigurations);
        }

        [Fact]
        public void GetNestedDependencies()
        {
            var dependencies = ModConfig.GetDependencies(TestData.TestModCConfig, TestData.AllMods);

            Assert.Contains(TestData.TestModAConfig, dependencies.Configurations);
            Assert.Contains(TestData.TestModBConfig, dependencies.Configurations);
        }

        [Fact]
        public void SortMods()
        {
            var dependencies  = ModConfig.GetDependencies(TestData.TestModCConfig, TestData.AllMods);
            var allMods       = new List<ModConfig>();

            allMods.Add(TestData.TestModCConfig);
            allMods.AddRange(dependencies.Configurations);
            allMods = ModConfig.SortMods(allMods);

            Assert.Equal(TestData.TestModAConfig, allMods[0]);
            Assert.Equal(TestData.TestModBConfig, allMods[1]);
            Assert.Equal(TestData.TestModCConfig, allMods[2]);
        }
    }
}
