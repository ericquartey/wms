using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMoveLoadingUnitMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        DateTimeOffset CreationDate { get; }

        LoadingUnitLocation Destination { get; }

        int? DestinationCellId { get; set; }

        bool InsertLoadUnit { get; }

        int? LoadUnitHeight { get; set; }

        int? LoadUnitId { get; }

        int? MissionId { get; }

        MissionStep MissionStep { get; }

        MissionType MissionType { get; set; }

        int? NetWeight { get; set; }

        LoadingUnitLocation Source { get; }

        int? SourceCellId { get; }

        StopRequestReason StopReason { get; }

        int? WmsId { get; set; }

        #endregion
    }
}
