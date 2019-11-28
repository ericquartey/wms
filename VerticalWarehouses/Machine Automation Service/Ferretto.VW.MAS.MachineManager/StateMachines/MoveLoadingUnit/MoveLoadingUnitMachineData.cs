using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitMachineData : IMoveLoadingUnitMachineData
    {
        #region Constructors

        internal MoveLoadingUnitMachineData(Guid id)
        {
            this.Id = id;
        }

        #endregion

        #region Properties

        public Guid Id { get; protected set; }

        public int? LoadingUnitCellSourceId { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
