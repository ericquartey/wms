using System;
using System.Runtime.Serialization;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Exceptions
{
    public class AutomationServiceException : Exception
    {
        #region Constructors

        public AutomationServiceException()
        {
        }

        public AutomationServiceException(string message)
            : base(message)
        {
        }

        public AutomationServiceException(string message, AutomationServiceExceptionCode exceptionEnum)
            : base(message)
        {
            this.AutomationServiceExceptionCode = exceptionEnum;
        }

        public AutomationServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AutomationServiceException(string message, AutomationServiceExceptionCode exceptionEnum, Exception inner)
            : base(message, inner)
        {
            this.AutomationServiceExceptionCode = exceptionEnum;
        }

        protected AutomationServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public AutomationServiceExceptionCode AutomationServiceExceptionCode { get; protected set; }

        #endregion
    }
}
