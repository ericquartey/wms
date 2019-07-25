using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class IoExceptionFieldMessageData : IIoExceptionFieldMessageData
    {
        #region Constructors

        public IoExceptionFieldMessageData(Exception innerException, string exceptionDescription, int exceptionCode, MessageVerbosity verbosity = MessageVerbosity.Debug)
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

        #region Methods

        public override string ToString()
        {
            return $"Code:{this.ExceptionCode} Description:{this.ExceptionDescription}";
        }

        #endregion
    }
}
