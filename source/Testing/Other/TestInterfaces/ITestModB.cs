namespace TestInterfaces
{
    public interface ITestModB
    {
        void ModifyControllerValueFromTestModA(int newValue);
        int UsePluginFromTestModA(int value);
    }
}