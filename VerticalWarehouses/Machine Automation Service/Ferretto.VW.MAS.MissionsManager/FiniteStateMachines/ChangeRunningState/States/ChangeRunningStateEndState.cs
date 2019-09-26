// ReSharper disable ArrangeThisQualifier
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateEndState : StateBase, IChangeRunningStateEndState
    {


        #region Constructors

        public ChangeRunningStateEndState(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory) { }

        #endregion
    }
}
