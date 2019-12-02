using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitMachineData : Mission, IMoveLoadingUnitMachineData
    {
        #region Constructors

        internal MoveLoadingUnitMachineData(Guid id)
        {
            this.FsmId = id;
        }

        #endregion
    }
}
