using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class MissionMessageData : IMissionMessageData
    {
        #region Constructors

        public MissionMessageData(int bayId, int cellId, int drawerId, MissionType missionType, int priority,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.BayID = bayId;
            this.CellID = cellId;
            this.DrawerID = drawerId;
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
