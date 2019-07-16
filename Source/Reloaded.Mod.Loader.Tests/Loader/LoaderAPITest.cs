using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Reloaded.Mod.Loader.Tests.SETUP;
using TestInterfaces;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Loader
{
    public class LoaderAPITest : IDisposable
    {
        private static Random _random = new Random();
        private Mod.Loader.Loader _loader;
        private TestData _testData;

        public LoaderAPITest()
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
        public void ModifyController()
        {
            // This also tests object sharing between load contexts.
            var testModBInstance = _loader.Manager.GetLoadedMods()[1];
            var testModAInstance = _loader.Manager.GetLoadedMods()[0];

            var testModB = (ITestModB)testModBInstance.Mod;
            var testModA = (ITestModA)testModAInstance.Mod;

            // Ask Mod B to modify controller from Mod A
            int random = _random.Next(0, int.MaxValue); // Note: Value in controller is initialized negative.
            testModB.ModifyControllerValueFromTestModA(random);

            int newValue = testModA.GetControllerValue();
            Assert.Equal(random, newValue);
        }

        [Fact]
        public void RemoveController()
        {
            _loader.Manager.LoaderApi.RemoveController<IController>();
            Assert.Null(_loader.Manager.LoaderApi.GetController<IController>());
        }

        [Fact]
        public void AutoDisposeController()
        {
            _loader.UnloadMod(_testData.TestModConfigA.ModId);
            Assert.Null(_loader.Manager.LoaderApi.GetController<IController>());
        }

        [Fact]
        public void UsePlugin()
        {
            var testModB = (ITestModB)_loader.Manager.GetLoadedMods()[1].Mod;
            int random = _random.Next(0, int.MaxValue / 2);

            int expected = random * 2;
            int actual = testModB.UsePluginFromTestModA(random);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoDisposePlugin()
        {
            // Get Mod B
            var testModB = (ITestModB)_loader.Manager.GetLoadedMods()[1].Mod;

            // Unload Mod A
            _loader.UnloadMod(_testData.TestModConfigA.ModId);

            // Weak reference in TestModB (Plugin from TestModA) should be invalid now.
            Assert.Throws<NullReferenceException>(() => testModB.UsePluginFromTestModA(0));
        }
    }
}
