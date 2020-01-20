using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.data = data;
            this.inverterStatus = inverterStatus;
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
            this.CurrentState = new PositioningStartState(this, this.data, this.inverterStatus, this.Logger);
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
