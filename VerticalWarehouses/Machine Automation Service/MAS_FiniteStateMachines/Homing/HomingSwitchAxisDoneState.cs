using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(IStateMachine parentMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Destructors

        ~HomingSwitchAxisDoneState()
        {
            this.Dispose( false );
        }

        #endregion

        /// <inheritdoc/>

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Command Message {message.Type} Source {message.Source}" );
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );
            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );

            if (message.Type == FieldMessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState( new HomingCalibrateAxisDoneState( this.ParentStateMachine, this.axisToCalibrate, this.logger ) );
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState( new HomingErrorState( this.ParentStateMachine, this.axisToCalibrate, message, this.logger ) );
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            var calibrateAxisData = new CalibrateAxisFieldMessageData( this.axisToCalibrate );
            var commandMessage = new FieldCommandMessage( calibrateAxisData,
                $"Homing {axisToCalibrate} State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.CalibrateAxis );

            this.logger.LogTrace( $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( commandMessage );

            var notificationMessageData = new CalibrateAxisMessageData( this.axisToCalibrate, MessageVerbosity.Info );
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.axisToCalibrate} axis calibration started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationStart );

            this.logger.LogTrace( $"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );

            this.ParentStateMachine.ChangeState( new HomingEndState( this.ParentStateMachine, this.axisToCalibrate, this.logger, true ) );
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

            base.Dispose( disposing );
        }

        #endregion
    }
}
