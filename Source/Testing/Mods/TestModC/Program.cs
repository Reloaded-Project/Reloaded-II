using System;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using TestInterfaces;

namespace TestModC
{
    public class Program : IModV1, ITestHelper
    {
        public string MyId { get; set; } = "TestModC";
        public bool ResumeExecuted { get; set; }
        public bool SuspendExecuted { get; set; }

        public const string NonexistingDependencyName = "TestModZ";

        /* Entry point. */
        public Action Disposing { get; }
        public void Start(IModLoaderV1 loader)
        {

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
    }
}
