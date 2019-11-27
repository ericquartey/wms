using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitMachineData : IMoveLoadingUnitMachineData
    {
        #region Constructors

        internal MoveLoadingUnitMachineData(Guid machineId)
        {
            this.MachineId = machineId;
        }

        #endregion

        #region Properties

        public int? LoadingUnitCellSourceId { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public Guid MachineId { get; protected set; }

        public MissionType MissionType { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
