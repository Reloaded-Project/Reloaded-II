using System;
using Reloaded.Mod.Loader.Exceptions;
using Reloaded.Mod.Loader.Server.Messages.Structures;
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
                ITestHelper sayHello = iMod as ITestHelper;
                if (sayHello == null)
                    Assert.True(false, "Failed to cast Test Mod.");

                bool isKnownConfig = sayHello.MyId == _testData.TestModConfigA.ModId ||
                                     sayHello.MyId == _testData.TestModConfigB.ModId;

                // TestMod C is unloaded, do not check for it.
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

            var testModA = (ITestHelper) loadedMods[0].Mod;
            Assert.Equal(_testData.TestModConfigA.ModId, testModA.MyId);

            var testModB = (ITestHelper) loadedMods[1].Mod;
            Assert.Equal(_testData.TestModConfigB.ModId, testModB.MyId);
        }

        [Fact]
        public void LoadNewMod()
        {
            _loader.LoadMod(_testData.TestModConfigC.ModId);
            var loadedMods = _loader.Manager.GetLoadedMods();

            // Should be loaded last.
            var testModC = (ITestHelper)loadedMods[2].Mod;
            Assert.Equal(_testData.TestModConfigC.ModId, testModC.MyId);

            // Check state consistency
            Assert.Equal(ModState.Running, loadedMods[2].State);
        }

        [Fact]
        public void CheckDefaultState()
        {
            foreach (var modInstance in _loader.Manager.GetLoadedMods())
            {
                Assert.Equal(ModState.Running, modInstance.State);
            }
        }

        [Fact]
        public void SuspendAll()
        {
            foreach (var modInstance in _loader.Manager.GetLoadedMods())
            {
                modInstance.Suspend();
                Assert.Equal(ModState.Suspended, modInstance.State);
                Assert.True(((ITestHelper)modInstance.Mod).SuspendExecuted);
            }
        }

        [Fact]
        public void SuspendAndResumeAll()
        {
            foreach (var modInstance in _loader.Manager.GetLoadedMods())
            {
                modInstance.Suspend();
                modInstance.Resume();
                Assert.Equal(ModState.Running, modInstance.State);
                Assert.True(((ITestHelper)modInstance.Mod).SuspendExecuted);
                Assert.True(((ITestHelper)modInstance.Mod).ResumeExecuted);
            }
        }

        [Fact]
        public void LoadInvalidMod()
        {
            Assert.Throws<ReloadedException>(() => _loader.LoadMod(""));
        }

        [Fact]
        public void LoadDuplicate()
        {
            Assert.Throws<ReloadedException>(() => _loader.LoadMod(_testData.TestModConfigB.ModId));
        }
    }
}
