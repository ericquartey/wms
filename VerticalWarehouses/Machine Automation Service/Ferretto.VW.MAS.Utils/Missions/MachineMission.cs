using System;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class MachineMission<TMachine> : Mission<TMachine>
        where TMachine : class, IFiniteStateMachine
    {
        #region Constructors

        public MachineMission(IServiceScopeFactory serviceScopeFactory, EventHandler<FiniteStateMachinesEventArgs> endHandler)
            : base(serviceScopeFactory)
        {
            this.CurrentStateMachine.Completed += endHandler;
        }

        #endregion

        #region Properties

        public TMachine MissionMachine => this.CurrentStateMachine;

        #endregion
    }
}
