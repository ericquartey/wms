using System;

namespace Ferretto.VW.Common_Utils
{
    public class InMemoryDataLayerException : DataLayerException
    {
        #region Constructors

        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum) : base(exceptionEnum)
        {
        }

        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum, String message) : base(message)
        {
            this.ConfigurationExceptionCode = exceptionEnum;
        }

        public InMemoryDataLayerException(DataLayerExceptionEnum exceptionEnum, String message,
            Exception inner) : base(message, inner)
        {
            this.ConfigurationExceptionCode = exceptionEnum;
        }

        #endregion
    }
}
