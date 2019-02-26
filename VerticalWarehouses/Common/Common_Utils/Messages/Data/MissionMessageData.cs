using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class MissionMessageData : IMissionMessageData
    {
        #region Constructors

        public MissionMessageData(Int32 bayID, Int32 cellID, Int32 drawerID, MissionType missionType, Int32 priority,
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

        public Int32 BayID { get; }

        public Int32 CellID { get; }

        public Int32 DrawerID { get; }

        public MissionType MissionType { get; }

        public Int32 Priority { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
