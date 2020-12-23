using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ShutterPositioning
{
    internal class ShutterPositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(
            IInverterShutterPositioningFieldMessageData shutterPositionData,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.shutterPositionData = shutterPositionData ?? throw new System.ArgumentNullException(nameof(shutterPositionData));
            this.inverterStatus = inverterStatus ?? throw new System.ArgumentNullException(nameof(inverterStatus));
        }

        #endregion

        #region Methods

        public override void Continue(double? targetPosition)
        {
            this.shutterPositionData.WaitContinue = false;
            this.Logger.LogDebug($"Continue command received for inverter {this.inverterStatus.SystemIndex}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.CurrentState = new ShutterPositioningStartState(this, this.inverterStatus, this.shutterPositionData, this.Logger);
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
