using System;
using System.Runtime.Serialization;

namespace Ferretto.VW.Common_Utils
{
    public class DataLayerException : Exception
    {
        #region Constructors

        public DataLayerException()
        {
        }

        public DataLayerException(string message) : base(message)
        {
        }

        public DataLayerException(DataLayerExceptionEnum exceptionEnum)
        {
            this.ConfigurationExceptionCode = exceptionEnum;
        }

        public DataLayerException(string message, DataLayerExceptionEnum exceptionEnum, Exception inner) : base(message, inner)
        {
            this.ConfigurationExceptionCode = exceptionEnum;
        }

        public DataLayerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DataLayerException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public DataLayerExceptionEnum ConfigurationExceptionCode { get; protected set; }

        #endregion
    }
}
