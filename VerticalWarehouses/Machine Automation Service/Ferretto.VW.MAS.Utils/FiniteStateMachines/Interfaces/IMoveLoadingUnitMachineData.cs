using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IMoveLoadingUnitMachineData : IFiniteStateMachineData
    {
        #region Properties

        // Used only if LoadingUnitSource in this object is LoadingUnitDestination.Cell
        int? LoadingUnitCellSourceId { get; set; }

        int LoadingUnitId { get; set; }

        // Used only if source in command is LoadingUnitDestination.LoadingUnit
        LoadingUnitLocation LoadingUnitSource { get; set; }

        MissionType MissionType { get; set; }

        BayNumber TargetBay { get; set; }

        int? WmsId { get; set; }

        #endregion
    }
}
