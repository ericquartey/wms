using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum MissionType
    {
        BayToCell,

        CellToBay,

        CellToCell
    }

    public interface IMissionMessageData : IEventMessageData
    {
        #region Properties

        int BayID { get; }

        int CellID { get; }

        int DrawerID { get; }

        MissionType MissionType { get; }

        int Priority { get; }

        #endregion
    }
}
