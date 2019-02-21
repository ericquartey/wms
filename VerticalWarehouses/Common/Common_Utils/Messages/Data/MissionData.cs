using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionData : IMissionMessageData
    {
        #region Properties

        public int BayID { get; set; }

        public int CellID { get; set; }

        public int DrawerID { get; set; }

        public MissionType MissionType { get; set; }

        public int Priority { get; set; }

        #endregion
    }
}
