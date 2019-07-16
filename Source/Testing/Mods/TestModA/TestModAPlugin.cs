using System;
using System.Collections.Generic;
using System.Text;
using TestInterfaces;

namespace TestModA
{
    public class TestModAPlugin : ITestModAPlugin
    {
        public int MultiplyByTwo(int value) => value * 2;
    }
}
