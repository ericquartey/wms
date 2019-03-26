using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private Axis currentAxis;

        private bool disposed;

        private bool IsStopRequested;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(Axis axisToCalibrate, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.logger.LogTrace($"CalibrateAxisStateMachine ctor");
            this.IsStopRequested = false;
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
        public override void PublishNotificationEvent(NotificationMessage message)
        {
            this.logger.LogTrace($"CalibrateAxisStateMachine publish notification {message.Type}");
            if (this.CurrentState is EndState)
            {
                var status = (this.IsStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;
                message.Status = status;
            }
            base.PublishNotificationEvent(message);
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.logger.LogTrace($"CalibrateAxisStateMachine start");
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
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.IsStopRequested = true;
            this.logger.LogTrace($"CalibrateAxisStateMachine stop");
            this.CurrentState.Stop();
        }

        #endregion
    }
}
