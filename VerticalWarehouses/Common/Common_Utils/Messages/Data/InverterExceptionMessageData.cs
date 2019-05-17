using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class InverterExceptionMessageData : IInverterExceptionMessageData
    {
        #region Constructors

        public InverterExceptionMessageData(Exception innerException, string exceptionDescription, int exceptionCode, MessageVerbosity verbosity = MessageVerbosity.Debug)
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
