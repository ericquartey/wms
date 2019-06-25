using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
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
    }
}
