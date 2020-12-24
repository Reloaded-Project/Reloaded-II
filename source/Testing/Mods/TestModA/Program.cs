using System;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using TestInterfaces;

namespace TestModA
{
    public class Program : IModV1, ITestHelper, ITestModA, IExports
    {
        public string MyId { get; set; } = "TestModA";
        public DateTime LoadTime { get; set; }
        public bool ResumeExecuted { get; set; }
        public bool SuspendExecuted { get; set; }

        private IController _controller;

        /* Entry point. */
        public Action Disposing { get; }
        public void Start(IModLoaderV1 loader)
        {
            LoadTime = DateTime.Now;
            _controller = new Controller();
            loader.AddOrReplaceController<IController>(this, _controller);
        }

        /* Suspend/Unload */
        public void Suspend()
        {
            SuspendExecuted = true;
        }

        public void Resume()
        {
            ResumeExecuted = true;
        }

        public void Unload()
        {
            
        }

        public bool CanUnload() => true;
        public bool CanSuspend() => true;

        /* Extensions */
        public int GetControllerValue() => _controller.Number;
        public Type[] GetTypes() => new []{ typeof(ITestModAPlugin), typeof(IController) };
    }
}
