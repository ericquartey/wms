using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault
{
    internal class ResetFaultStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public ResetFaultStateMachine(
            IInverterStatusBase inverterStatus,
            InverterIndex inverterIndex,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            if (inverterStatus is null)
            {
                throw new System.ArgumentNullException(nameof(inverterStatus));
            }

            this.inverterStatus = inverterStatus;
            this.inverterIndex = inverterIndex;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new ResetFaultStartState(this, this.inverterStatus, this.inverterIndex, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion

        //public override void Stop()
        //{
        //    this.CurrentState?.Stop();
        //}
    }
}
