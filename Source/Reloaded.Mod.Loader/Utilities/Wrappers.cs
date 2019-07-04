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
    }
}
