using System;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum MissionType
    {
        BayToCell,

        CellToBay,

        CellToCell
    }

    public interface IMissionMessageData : IMessageData
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
