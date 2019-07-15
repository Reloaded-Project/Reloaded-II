using System;

namespace TestInterfaces
{
    public interface ITestHelper
    {
        string MyId { get; set; }
        bool ResumeExecuted { get; set; }
        bool SuspendExecuted { get; set; }
    }
}
