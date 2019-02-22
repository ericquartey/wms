using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionMessageData : IMissionMessageData
    {
        #region Constructors

        public MissionData(int bayID, int cellID, int drawerID, MissionType missionType, int priority)
        {
            this.BayID = bayID;
            this.CellID = cellID;
            this.DrawerID = drawerID;
            this.MissionType = missionType;
            this.Priority = priority;
        }

        #endregion

        #region Properties

        public int BayID { get; private set; }

        public int CellID { get; private set; }

        public int DrawerID { get; private set; }

        public MissionType MissionType { get; private set; }

        public int Priority { get; private set; }

        #endregion
    }
}
