using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces
{
    internal interface IMoveLoadingUnitMachineData : IFiniteStateMachineData
    {
        #region Properties

        // Used only if LoadingUnitSource in this object is LoadingUnitDestination.Cell
        int? LoadingUnitCellSourceId { get; set; }

        int LoadingUnitId { get; set; }

        // Used only if source in command is LoadingUnitDestination.LoadingUnit
        LoadingUnitLocation LoadingUnitSource { get; set; }

        #endregion
    }
}
