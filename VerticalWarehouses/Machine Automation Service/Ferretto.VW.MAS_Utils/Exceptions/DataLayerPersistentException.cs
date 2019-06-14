using System;
using System.Runtime.Serialization;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Exceptions
{
    public class DataLayerPersistentException : Exception
    {
        #region Constructors

        public DataLayerPersistentException()
        {
        }

        public DataLayerPersistentException(string message)
            : base(message)
        {
        }

        public DataLayerPersistentException(DataLayerPersistentExceptionCode exception)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, DataLayerPersistentExceptionCode exception)
            : base(message)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, DataLayerPersistentExceptionCode exception, Exception inner)
            : base(message, inner)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DataLayerPersistentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public DataLayerPersistentExceptionCode ConfigurationExceptionCode { get; protected set; }

        #endregion
    }
}
