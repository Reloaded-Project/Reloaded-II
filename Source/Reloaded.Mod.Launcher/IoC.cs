using System.Linq;
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

        /// <summary>
        /// Retrieves a constant service/class.
        /// If none is registered, binds it as the new constant to then be re-acquired.
        /// </summary>
        public static T GetConstant<T>()
        {
            var value = Kernel.Get<T>();

            if (Kernel.GetBindings(typeof(T)).All(x => x.IsImplicit))
            {
                Kernel.Bind<T>().ToConstant(value);
            }

            return value;
        }
    }
}