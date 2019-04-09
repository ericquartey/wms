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

        private Axis currentAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(Axis axisToCalibrate, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.axisToCalibrate = axisToCalibrate;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;

            Logger.LogDebug("2:Method End");
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
            this.Logger.LogDebug("1:Method Start");

            this.Logger.LogTrace($"2:Type={message.Type}:Destination={message.Destination}:Status={message.Status}");

            base.PublishNotificationEvent(message);

            this.Logger.LogDebug("3:Method End");
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("1:Method Start");
            this.Logger.LogTrace($"2:Axis to calibrate={this.axisToCalibrate}");

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

            this.CurrentState = new VoltageDisabledState(this, this.currentAxis, this.Logger);

            this.Logger.LogDebug("3:Method End");
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Method Start");

            this.CurrentState.Stop();

            this.Logger.LogDebug("2:Method End");
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
