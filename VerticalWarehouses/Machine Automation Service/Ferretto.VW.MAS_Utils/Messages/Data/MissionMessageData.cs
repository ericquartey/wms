using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionMessageData : IMissionMessageData
    {
        #region Constructors

        public MissionMessageData(int bayID, int cellID, int drawerID, MissionType missionType, int priority,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.BayID = bayID;
            this.CellID = cellID;
            this.DrawerID = drawerID;
            this.MissionType = missionType;
            this.Priority = priority;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int BayID { get; }

        public int CellID { get; }

        public int DrawerID { get; }

        public MissionType MissionType { get; }

        public int Priority { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
