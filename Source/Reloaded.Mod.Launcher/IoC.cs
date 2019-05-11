using Ninject;

namespace Reloaded.Mod.Launcher
{
    public static class IoC
    {
        /// <summary>
        /// The standard NInject Kernel.
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();


        /// <summary>
        /// Sets up the IoC container with any relevant bindings on first access of the class.
        /// </summary>
        static IoC()
        {
            /* No code here yet. */
        }

        /// <summary>
        /// Retrieves a service (class) from the IoC of a specified type.
        /// </summary>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
}