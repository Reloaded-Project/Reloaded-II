using System;
using System.Collections.Generic;
using System.Text;

namespace TestInterfaces
{
    public interface ITestModB
    {
        void ModifyControllerValueFromTestModA(int newValue);
        int UsePluginFromTestModA(int value);
    }
}
