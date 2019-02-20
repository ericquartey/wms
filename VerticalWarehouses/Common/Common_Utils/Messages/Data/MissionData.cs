using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionData : IMissionMessageData
    {
        #region Properties

        public int Priority { get; set; }

        #endregion
    }
}
