using System;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState
{
    internal class ChangeRunningStateMachineData : IChangeRunningStateMachineData
    {
        #region Constructors

        internal ChangeRunningStateMachineData(Guid machineId)
        {
            this.MachineId = machineId;
        }

        #endregion

        #region Properties

        public Guid MachineId { get; }

        #endregion
    }
}
