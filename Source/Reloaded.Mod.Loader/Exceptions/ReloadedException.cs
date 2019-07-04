using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Reloaded.Mod.Loader.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ReloadedException : System.Exception
    {
        public ReloadedException()
        {
        }

        protected ReloadedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ReloadedException(string message) : base(message)
        {
        }

        public ReloadedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
