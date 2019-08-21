using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class DLExceptionMessageData : IDLExceptionMessageData
    {
        #region Constructors

        public DLExceptionMessageData(Exception innerException, string exceptionDescription = null, int exceptionCode = 0, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InnerException = innerException;
            this.ExceptionDescription = exceptionDescription;
            this.ExceptionCode = exceptionCode;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int ExceptionCode { get; }

        public string ExceptionDescription { get; }

        public Exception InnerException { get; }

        public MessageVerbosity Verbosity { get;  }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Code:{this.ExceptionCode} Description:{this.ExceptionDescription}";
        }

        #endregion
    }
}
