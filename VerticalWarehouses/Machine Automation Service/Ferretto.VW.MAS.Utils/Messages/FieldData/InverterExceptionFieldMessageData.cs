using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterExceptionFieldMessageData : FieldMessageData, IInverterExceptionFieldMessageData
    {
        #region Constructors

        public InverterExceptionFieldMessageData(
            Exception innerException,
            string exceptionDescription,
            int exceptionCode,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
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

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Code:{this.ExceptionCode} Description:{this.ExceptionDescription}";
        }

        #endregion
    }
}
