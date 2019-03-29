using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
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
            this.logger.LogDebug("1:Method Start");

            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
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
        public override void OnPublishNotification(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Type={message.Type}:Destination={message.Destination}:Status={message.Status}");

            switch (message.Type)
            {
                case MessageType.CalibrateAxis:
                    {
                        //TEMP Send a notification about the start operation to all the world
                        var status = (this.IsStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;

                        var endNotification = new NotificationMessage(message.Data,
                            message.Description,
                            MessageActor.Any,
                            MessageActor.InverterDriver,
                            MessageType.CalibrateAxis,
                            status
                        );

                        this.logger.LogTrace($"3:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                        base.PublishNotificationEvent(endNotification);
                        break;
                    }

                case MessageType.Stop:
                    {
                        //var msgStatus = (this.IsStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;

                        ////TEMP Send a notification about the end (/stop) operation to all the world
                        //var newMessage = new NotificationMessage(null,
                        //    "End Homing",
                        //    MessageActor.Any,
                        //    MessageActor.FiniteStateMachines,
                        //    MessageType.Stop,
                        //    msgStatus,
                        //    ErrorLevel.NoError,
                        //    MessageVerbosity.Info);

                        //this.eventAggregator.GetEvent<NotificationEvent>().Publish(newMessage);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            this.logger.LogDebug("3:Method End");
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
            this.IsStopRequested = true;
            this.logger.LogTrace($"Stop");
            this.CurrentState.Stop();
        }

        #endregion
    }
}
