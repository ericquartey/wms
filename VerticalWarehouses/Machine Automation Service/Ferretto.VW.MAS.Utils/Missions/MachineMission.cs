using System;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class MachineMission<TMachine> : Mission<TMachine>
        where TMachine : class, IFiniteStateMachine
    {
        #region Constructors

        public MachineMission(TMachine finiteStateMachine, EventHandler<FiniteStateMachinesEventArgs> endHandler)
            : base(finiteStateMachine.InstanceId, finiteStateMachine)
        {
            this.CurrentStateMachine.Completed += endHandler;
        }

        #endregion

        #region Properties

        public TMachine MissionMachine => this.CurrentStateMachine;

        #endregion
    }
}
