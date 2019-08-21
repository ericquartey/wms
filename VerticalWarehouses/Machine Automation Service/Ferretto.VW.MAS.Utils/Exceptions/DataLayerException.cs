using System;
using System.Runtime.Serialization;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Exceptions
{
    public class DataLayerException : Exception
    {
        #region Constructors

        public DataLayerException()
        {
        }

        public DataLayerException(string message)
            : base(message)
        {
        }

        public DataLayerException(DataLayerExceptionCode exception)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerException(string message, DataLayerExceptionCode exception, Exception inner)
            : base(message, inner)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DataLayerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public DataLayerExceptionCode ConfigurationExceptionCode { get; protected set; }

        #endregion
    }
}
