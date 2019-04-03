using System;
using System.Runtime.Serialization;
using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils
{
    public class DataLayerPersistentException : Exception
    {
        #region Constructors

        public DataLayerPersistentException()
        {
        }

        public DataLayerPersistentException(string message) : base(message)
        {
        }

        public DataLayerPersistentException(DataLayerPersistentExceptionCode exception)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, DataLayerPersistentExceptionCode exception) : base(message)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, DataLayerPersistentExceptionCode exception, Exception inner) : base(message, inner)
        {
            this.ConfigurationExceptionCode = exception;
        }

        public DataLayerPersistentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DataLayerPersistentException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public DataLayerPersistentException ConfigurationExceptionCode { get; protected set; }

        #endregion

        #region Methods

        public static implicit operator DataLayerPersistentException(DataLayerPersistentExceptionCode v)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
