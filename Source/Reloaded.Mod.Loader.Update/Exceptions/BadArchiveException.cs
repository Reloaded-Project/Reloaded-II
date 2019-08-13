using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Reloaded.Mod.Loader.Update.Exceptions
{
    public class BadArchiveException : Exception
    {
        public BadArchiveException() { }
        protected BadArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public BadArchiveException(string message) : base(message) { }
        public BadArchiveException(string message, Exception innerException) : base(message, innerException) { }
    }
}
