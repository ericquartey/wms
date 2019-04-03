using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private Axis currentAxis;

        private bool disposed;

        private bool isStopRequested;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(Axis axisToCalibrate, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.axisToCalibrate = axisToCalibrate;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;
            this.logger = logger;
            this.isStopRequested = false;

            logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~CalibrateAxisStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void PublishNotificationEvent(FieldNotificationMessage message)
        {
            if (this.CurrentState is EndState)
            {
                var status = (this.isStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;
                message.Status = status;
            }

            this.logger.LogTrace($"1:Type={message.Type}:Destination={message.Destination}:Status={message.Status}");

            base.PublishNotificationEvent(message);
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Axis to calibrate={this.axisToCalibrate}");

            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                case Axis.Horizontal:
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new VoltageDisabledState(this, this.currentAxis, this.logger);

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.isStopRequested = true;
            this.CurrentState.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
