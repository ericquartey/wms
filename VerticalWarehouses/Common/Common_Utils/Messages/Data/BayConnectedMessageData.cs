using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class BayConnectedMessageData : IBayConnectedMessageData
    {
        #region Properties

        public int BayType { get; set; }

        public int Id { get; set; }

        public int MissionQuantity { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
