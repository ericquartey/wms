using System;

namespace Ferretto.VW.Common_Utils
{
    public class ExceptionsUtils : Exception
    {
        public ExceptionsUtils() : base() { }

        public ExceptionsUtils(string message) : base(message) { }

        public ExceptionsUtils(string message, System.Exception inner) : base(message, inner) { }

        protected ExceptionsUtils(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
