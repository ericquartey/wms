using System;

namespace Ferretto.VW.Common_Utils
{
    public class DataLayerException : Exception
    {
        public DataLayerExceptionEnum ConfigurationExceptionCode { get; protected set; }

        public DataLayerException() : base() { }

        public DataLayerException(string message) : base(message) { }

        public DataLayerException(DataLayerExceptionEnum exceptionEnum) : base()
        {
            ConfigurationExceptionCode = exceptionEnum;
        }

        public DataLayerException(string message, System.Exception inner) : base(message, inner) { }

        protected DataLayerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
