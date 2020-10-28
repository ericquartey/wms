using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayOperationalStatusChangedMessageData : IMessageData
    {
        #region Constructors

        public BayOperationalStatusChangedMessageData()
        {
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        public BayStatus BayStatus { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
