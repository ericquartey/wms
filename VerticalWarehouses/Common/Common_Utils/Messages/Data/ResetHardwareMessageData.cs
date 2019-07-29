using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ResetHardwareMessageData : IResetHardwareMessageData
    {
        #region Constructors

        public ResetHardwareMessageData(ResetOperation operation, MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Operation = operation;

            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public ResetOperation Operation { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
