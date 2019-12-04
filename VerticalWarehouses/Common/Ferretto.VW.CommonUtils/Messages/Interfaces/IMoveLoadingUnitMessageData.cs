using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMoveLoadingUnitMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        LoadingUnitLocation Destination { get; }

        int? DestinationCellId { get; set; }

        bool EjectLoadingUnit { get; }

        bool InsertLoadingUnit { get; }

        int? LoadingUnitId { get; }

        Guid? MissionId { get; }

        MissionType MissionType { get; set; }

        LoadingUnitLocation Source { get; }

        int? SourceCellId { get; }

        StopRequestReason StopReason { get; }

        BayNumber TargetBay { get; set; }

        int? WmsId { get; set; }

        #endregion
    }
}
