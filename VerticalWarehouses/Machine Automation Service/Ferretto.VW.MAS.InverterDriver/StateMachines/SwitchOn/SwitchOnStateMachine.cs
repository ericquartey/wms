using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn
{
    internal class SwitchOnStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public SwitchOnStateMachine(
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.inverterStatus = inverterStatus;
            this.axisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new SwitchOnStartState(this, this.axisToSwitchOn, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion

        //public override void Stop()
        //{
        //    this.CurrentState?.Stop();
        //}
    }
}
