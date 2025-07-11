﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IMoveLoadingUnitMachineData : IFiniteStateMachineData
    {
        #region Properties

        DateTime CreationDate { get; set; }

        int? DestinationCellId { get; set; }

        string FsmStateName { get; set; }

        // Used only if LoadingUnitSource in this object is LoadingUnitDestination.Cell
        int? LoadingUnitCellSourceId { get; set; }

        LoadingUnitLocation LoadingUnitDestination { get; set; }

        int LoadingUnitId { get; set; }

        LoadingUnitLocation LoadingUnitSource { get; set; }

        MissionType MissionType { get; set; }

        int Priority { get; set; }

        MissionStatus Status { get; set; }

        BayNumber TargetBay { get; set; }

        int? WmsId { get; set; }

        #endregion
    }
}
