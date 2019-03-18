using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class UpDownRepetitiveNotificationMessageData : IUpDownRepetitiveNotificationMessageData
    {
        #region Constructors

        public UpDownRepetitiveNotificationMessageData(int numberOfCompletedCycles, MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.NumberOfCompletedCycles = this.NumberOfCompletedCycles;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int NumberOfCompletedCycles { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
