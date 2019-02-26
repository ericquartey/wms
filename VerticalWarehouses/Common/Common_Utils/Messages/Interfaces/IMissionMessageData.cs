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

        Int32 BayID { get; }

        Int32 CellID { get; }

        Int32 DrawerID { get; }

        MissionType MissionType { get; }

        Int32 Priority { get; }

        #endregion
    }
}
