using System;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState
{
    internal class ChangeRunningStateMachineData : IChangeRunningStateMachineData
    {
        #region Constructors

        internal ChangeRunningStateMachineData(Guid machineId)
        {
            this.Id = machineId;
        }

        #endregion

        #region Properties

        public Guid Id { get; }

        #endregion
    }
}
