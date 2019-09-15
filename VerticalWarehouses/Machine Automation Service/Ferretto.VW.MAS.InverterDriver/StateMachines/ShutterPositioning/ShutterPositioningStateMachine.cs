using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
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
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IInverterStatusBase inverterStatus,
              IEventAggregator eventAggregator,
              ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue)
        {
            this.shutterPositionData = shutterPositionData;
            this.inverterStatus = inverterStatus;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.CurrentState = new ShutterPositioningStartState(this, this.inverterStatus, this.shutterPositionData, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState?.Stop();
        }

        #endregion
    }
}
