using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitMachineData : IMoveLoadingUnitMachineData
    {
        #region Properties

        public bool IsMovingBayChain { get; set; }

        public bool IsMovingHorizontal { get; set; }

        public bool IsMovingShutter { get; set; }

        public bool IsMovingVertical { get; set; }

        public int? LoadingUnitCellSourceId { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        #endregion
    }
}
