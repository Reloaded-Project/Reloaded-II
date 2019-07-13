using System.Collections.Generic;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.Tests.SETUP
{
    public class ModConfigTestData
    {
        public List<ModConfig> AllMods { get; set; } = new List<ModConfig>();
        public List<string> NonexistingDependencies { get; set; } = new List<string>();
        public ModConfig TestModAConfig { get; set; }
        public ModConfig TestModBConfig { get; set; }
        public ModConfig TestModCConfig { get; set; }

        public ModConfigTestData()
        {
            TestModAConfig = TestModA.Program.ModConfig;
            TestModBConfig = TestModB.Program.ModConfig;
            TestModCConfig = TestModC.Program.ModConfig;

            NonexistingDependencies.Add(TestModB.Program.NonexistingDependencyName);
            NonexistingDependencies.Add(TestModC.Program.NonexistingDependencyName);

            AllMods.Add(TestModA.Program.ModConfig);
            AllMods.Add(TestModB.Program.ModConfig);
            AllMods.Add(TestModC.Program.ModConfig);
        }
    }
}
