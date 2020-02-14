using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableStateMachine : InverterStateMachineBase
    {
        #region Fields

        public readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterPositioningFieldMessageData dataOld;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public PositioningTableStateMachine(
            IInverterPositioningFieldMessageData data,
            IInverterPositioningFieldMessageData dataOld,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            if (data is null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            if (inverterStatus is null)
            {
                throw new System.ArgumentNullException(nameof(inverterStatus));
            }

            this.data = data;
            this.inverterStatus = inverterStatus;
            this.dataOld = dataOld;
        }

        #endregion

        #region Methods

        public override void Continue()
        {
            this.data.WaitContinue = false;
            this.Logger.LogDebug($"Continue command received for inverter {this.inverterStatus.SystemIndex}");
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new PositioningTableStartState(this, this.data, this.dataOld, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion

        ///// <inheritdoc />
        //public override void Stop()
        //{
        //    this.CurrentState?.Stop();
        //}
    }
}
