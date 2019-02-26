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

        public DataLayerException(String message) : base(message)
        {
        }

        public DataLayerException(DataLayerExceptionEnum exceptionEnum)
        {
            this.ConfigurationExceptionCode = exceptionEnum;
        }

        public DataLayerException(String message, Exception inner) : base(message, inner)
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
