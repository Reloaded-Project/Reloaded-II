using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.Mod.Loader.Tests.SETUP;
using TestInterfaces;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Loader
{
    public class ServerTest : IDisposable
    {
        private static Random _random = new Random();
        private Mod.Loader.Loader _loader;
        private TestData _testData;
        private Host _host;
        private Client _client;

        public ServerTest()
        {
            _testData = new TestData();

            _loader = new Mod.Loader.Loader(true);
            _loader.LoadForCurrentProcess();

            _host = new Host(_loader);
            _client = new Client(_host.Port);
        }

        public void Dispose()
        {
            _testData?.Dispose();
            _loader?.Dispose();
        }

        [Fact]
        public void LoadedModsCount()
        {
            Task<GetLoadedModsResponse> response = _client.GetLoadedModsAsync();
            response.Wait();

            int loadedMods = _loader.Manager.GetLoadedMods().Length;
            Assert.Equal(loadedMods, response.Result.Mods.Length);
        }

        [Fact]
        public void AssertLoadedMods()
        {
            Task<GetLoadedModsResponse> response = _client.GetLoadedModsAsync();
            response.Wait();

            ModInstance[]   localLoadedMods = _loader.Manager.GetLoadedMods();
            ModInfo[]       remoteLoadedMods = response.Result.Mods;

            for (int x = 0; x < remoteLoadedMods.Length; x++)
            {
                Assert.Equal(localLoadedMods[x].State           , remoteLoadedMods[x].State);
                Assert.Equal(localLoadedMods[x].ModConfig.ModId , remoteLoadedMods[x].ModId);
                Assert.Equal(localLoadedMods[x].CanSuspend      , remoteLoadedMods[x].CanSuspend);
                Assert.Equal(localLoadedMods[x].CanUnload       , remoteLoadedMods[x].CanUnload);
            }
        }

        [Fact]
        public void LoadMod()
        {
            var loadModTask = _client.LoadMod(_testData.TestModConfigC.ModId);
            loadModTask.Wait();

            // Should be loaded last.
            var loadedMods = _loader.Manager.GetLoadedMods();
            var testModC = (ITestHelper)   loadedMods.Last().Mod;
            Assert.Equal(_testData.TestModConfigC.ModId, testModC.MyId);

            // Check state consistency
            Assert.Equal(ModState.Running, loadedMods.Last().State);
        }

        [Fact]
        public void LoadDuplicateMod()
        {
            bool testPassed = false;
            _client.OnReceiveException += response => { testPassed = true; };

            Assert.Throws<AggregateException>(() =>
            {
                var result = _client.LoadMod(_testData.TestModConfigA.ModId);
                result.Wait();
            });
            
            Assert.True(testPassed);
        }

        [Fact]
        public void UnloadMod()
        {
            var unloadModTask = _client.UnloadModAsync(_testData.TestModConfigB.ModId);
            unloadModTask.Wait();

            // Should be loaded last.
            var loadedMods        = _loader.Manager.GetLoadedMods();
            ModInstance configB   = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testData.TestModConfigB.ModId);

            Assert.Null(configB);
        }

        [Fact]
        public void SuspendMod()
        {
            var suspendModTask = _client.SuspendModAsync(_testData.TestModConfigB.ModId);
            suspendModTask.Wait();

            // Get instance for B
            var loadedMods = _loader.Manager.GetLoadedMods();
            ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testData.TestModConfigB.ModId);

            Assert.Equal(ModState.Suspended, configB.State);
        }

        [Fact]
        public void SuspendAndResumeMod()
        {
            // Suspend first.
            SuspendMod();

            // Now resume.
            var resumeModTask = _client.ResumeModAsync(_testData.TestModConfigB.ModId);
            resumeModTask.Wait();

            // Get instance for B
            var loadedMods      = _loader.Manager.GetLoadedMods();
            ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testData.TestModConfigB.ModId);

            Assert.Equal(ModState.Running, configB.State);
        }
    }
}
