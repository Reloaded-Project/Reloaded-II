using System.Collections.Generic;
using Reloaded.Mod.Loader.Exceptions;

namespace Reloaded.Mod.Loader.Utilities
{
    public static class Wrappers
    {
        public static void ThrowIfNull(object thing, string message)
        {
            if (thing == null)
                throw new ReloadedException(message);
        }

        public static void ThrowIfENotEqual<T>(T value, T expected, string message) where T : unmanaged
        {
            if (! EqualityComparer<T>.Default.Equals(value, expected))
                throw new ReloadedException(message);
        }
    }
}
