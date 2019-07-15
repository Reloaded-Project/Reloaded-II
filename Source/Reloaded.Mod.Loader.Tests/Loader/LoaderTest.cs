using System;
using Reloaded.Mod.Loader.Tests.SETUP;
using TestInterfaces;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Loader
{
    public class LoaderTest : IDisposable
    {
        private Mod.Loader.Loader _loader;
        private TestData _testData;

        public LoaderTest()
        {
            _testData = new TestData();

            _loader = new Mod.Loader.Loader();
            _loader.LoadForCurrentProcess();
        }

        public void Dispose()
        {
            _testData?.Dispose();
            _loader?.Dispose();
        }

        [Fact]
        public void ExecuteCodeFromLoadedMods()
        {
            var loadedMods = _loader.Manager.GetLoadedMods();
            foreach (var mod in loadedMods)
            {
                var iMod = mod.Mod;
                IIdentifyMyself sayHello = iMod as IIdentifyMyself;
                if (sayHello == null)
                    Assert.True(false, "Failed to cast Test Mod.");

                bool isKnownConfig = sayHello.MyId == _testData.TestModConfigA.ModId ||
                                     sayHello.MyId == _testData.TestModConfigB.ModId ||
                                     sayHello.MyId == _testData.TestModConfigC.ModId;

                Assert.True(isKnownConfig);
            }
        }

        [Fact]
        public void CountLoadedMods()
        {
            var loadedMods = _loader.Manager.GetLoadedMods();
            Assert.Equal(2, loadedMods.Length);
        }

        [Fact]
        public void CheckLoadOrder()
        {
            var loadedMods = _loader.Manager.GetLoadedMods();

            var testModA = (IIdentifyMyself) loadedMods[0].Mod;
            Assert.Equal(_testData.TestModConfigA.ModId, testModA.MyId);

            var testModB = (IIdentifyMyself) loadedMods[1].Mod;
            Assert.Equal(_testData.TestModConfigB.ModId, testModB.MyId);
        }
    }
}
