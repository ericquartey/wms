using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterExceptionFieldMessageData : IInverterExceptionFieldMessageData
    {
        #region Constructors

        public InverterExceptionFieldMessageData(Exception innerException, string exceptionDescription, int exceptionCode, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InnerException = innerException;
            this.ExceptionDescription = exceptionDescription;
            this.ExceptionCode = exceptionCode;
        }

        #endregion

        #region Properties

        public int ExceptionCode { get; }

        public string ExceptionDescription { get; }

        public Exception InnerException { get; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
