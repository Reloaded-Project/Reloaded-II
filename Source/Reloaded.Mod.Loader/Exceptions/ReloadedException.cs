using System.Diagnostics.CodeAnalysis;

namespace Reloaded.Mod.Loader.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ReloadedException : System.Exception
    {
        public ReloadedException(string message) : base(message)
        {
        }
    }
}
