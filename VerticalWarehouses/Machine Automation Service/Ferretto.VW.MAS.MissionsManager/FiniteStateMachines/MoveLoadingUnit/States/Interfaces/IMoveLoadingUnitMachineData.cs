using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces
{
    internal interface IMoveLoadingUnitMachineData : IFiniteStateMachineData
    {
        #region Properties

        int? LoadingUnitId { get; set; }

        #endregion
    }
}
