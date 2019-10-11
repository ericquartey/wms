using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitMachineData : IMoveLoadingUnitMachineData
    {
        #region Properties

        public int? LoadingUnitId { get; set; }

        #endregion
    }
}
