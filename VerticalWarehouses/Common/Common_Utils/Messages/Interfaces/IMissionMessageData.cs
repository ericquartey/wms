using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum MissionType
    {
        BayToCellMission,

        CellToBayMission,

        CellToCellMission
    }

    public interface IMissionMessageData : IEventMessageData
    {
        #region Properties

        int BayID { get; set; }

        int CellID { get; set; }

        int DrawerID { get; set; }

        MissionType MissionType { get; set; }

        int Priority { get; set; }

        #endregion
    }
}
