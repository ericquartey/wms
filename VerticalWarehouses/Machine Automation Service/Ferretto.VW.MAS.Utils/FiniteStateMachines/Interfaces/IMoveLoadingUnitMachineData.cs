using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IMoveLoadingUnitMachineData : IFiniteStateMachineData
    {
        #region Properties

        DateTime CreationDate { get; set; }

        string FsmStateName { get; set; }

        // Used only if LoadingUnitSource in this object is LoadingUnitDestination.Cell
        int? LoadingUnitCellSourceId { get; set; }

        int LoadingUnitId { get; set; }

        // Used only if source in command is LoadingUnitDestination.LoadingUnit
        LoadingUnitLocation LoadingUnitSource { get; set; }

        MissionType MissionType { get; set; }

        int Priority { get; set; }

        MissionStatus Status { get; set; }

        BayNumber TargetBay { get; set; }

        int? WmsId { get; set; }

        #endregion
    }
}
