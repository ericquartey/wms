using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class WebApiExceptionMessageData : IWebApiExceptionMessageData
    {
        #region Constructors

        public WebApiExceptionMessageData(string exceptionDescription, int exceptionCode, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ExceptionDescription = exceptionDescription;
            this.ExceptionCode = exceptionCode;
        }

        #endregion

        #region Properties

        public int ExceptionCode { get; }

        public string ExceptionDescription { get; }

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
