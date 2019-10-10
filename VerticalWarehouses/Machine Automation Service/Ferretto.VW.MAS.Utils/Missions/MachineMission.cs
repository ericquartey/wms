using System;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class MachineMission : Mission
    {
        #region Constructors

        public MachineMission(IFiniteStateMachine finiteStateMachine, EventHandler<FiniteStateMachinesEventArgs> endHandler)
            : base(finiteStateMachine.InstanceId, finiteStateMachine)
        {
            this.CurrentStateMachine = finiteStateMachine;
            this.CurrentStateMachine.Completed += endHandler;
            this.MissionId = finiteStateMachine.InstanceId;
        }

        #endregion

        #region Properties

        public Guid Id => this.MissionId;

        public IFiniteStateMachine MissionMachine => this.CurrentStateMachine;

        #endregion
    }
}
