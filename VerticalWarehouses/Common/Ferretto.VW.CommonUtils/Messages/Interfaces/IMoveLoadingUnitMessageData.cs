﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMoveLoadingUnitMessageData : IMessageData
    {
        #region Properties

        CommandAction CommandAction { get; }

        LoadingUnitLocation Destination { get; }

        int? DestinationCellId { get; }

        bool EjectLoadingUnit { get; }

        bool InsertLoadingUnit { get; }

        int? LoadingUnitId { get; }

        Guid? MissionId { get; set; }

        LoadingUnitLocation Source { get; }

        int? SourceCellId { get; }

        StopRequestReason StopReason { get; }

        #endregion
    }
}
