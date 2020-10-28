using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ErrorStatusMessageData : IErrorStatusMessageData
    {
        #region Constructors

        public ErrorStatusMessageData()
        {
        }

        public ErrorStatusMessageData(
            int errorId,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ErrorId = errorId;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int ErrorId { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
