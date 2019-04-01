using System;
using System.Runtime.Serialization;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Exceptions
{
    public class MissionsManagerException : Exception
    {
        #region Constructors

        public MissionsManagerException()
        {
        }

        public MissionsManagerException(string message) : base(message)
        {
        }

        public MissionsManagerException(string message, MissionsManagerExceptionCode exceptionEnum) : base(message)
        {
            this.AutomationServiceExceptionCode = exceptionEnum;
        }

        public MissionsManagerException(string message, Exception inner) : base(message, inner)
        {
        }

        public MissionsManagerException(string message, MissionsManagerExceptionCode exceptionEnum, Exception inner) :
            base(message, inner)
        {
            this.AutomationServiceExceptionCode = exceptionEnum;
        }

        protected MissionsManagerException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region Properties

        public MissionsManagerExceptionCode AutomationServiceExceptionCode { get; protected set; }

        #endregion
    }
}
