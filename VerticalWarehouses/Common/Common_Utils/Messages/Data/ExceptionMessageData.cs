using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ExceptionMessageData : IExceptionMessageData
    {
        #region Constructors

        public ExceptionMessageData(Exception innerException, string description, int exceptionCode, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InnerException = innerException;
            this.Description = description;
            this.ExceptionCode = exceptionCode;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public int ExceptionCode { get; }

        public Exception InnerException { get; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
