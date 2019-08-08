using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    public class PositioningEndState : StateBase
    {
        #region Fields

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly int numberExecutedSteps;

        private readonly IPositioningMessageData positioningMessageData;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningEndState(
            IStateMachine parentMachine,
            IMachineSensorsStatus machineSensorStatus,
            IPositioningMessageData positioningMessageData,
            ILogger logger,
            int numberExecutedSteps,
            bool stopRequested = false )
            : base( parentMachine, logger )
        {
            this.stopRequested = stopRequested;
            this.positioningMessageData = positioningMessageData;
            this.machineSensorsStatus = machineSensorsStatus;
            this.numberExecutedSteps = numberExecutedSteps;
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage( FieldNotificationMessage message )
        {
            this.Logger.LogTrace( $"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}" );

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                               this.positioningMessageData,
                               this.positioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                               MessageActor.Any,
                               MessageActor.FiniteStateMachines,
                               MessageType.Positioning,
                               MessageStatus.OperationStop );

                            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState( new PositioningErrorState( this.ParentStateMachine, this.machineSensorsStatus, this.positioningMessageData, message, this.Logger ) );
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage( NotificationMessage message )
        {
            this.Logger.LogTrace( $"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.Logger?.LogTrace( "1:Method Start" );

            lock (this.machineSensorsStatus)
            {
                this.positioningMessageData.CurrentPosition = (this.positioningMessageData.AxisMovement == Axis.Vertical) ? this.machineSensorsStatus.AxisYPosition : this.machineSensorsStatus.AxisXPosition;
            }

            if (this.stopRequested)
            {
                //TEMP The FSM must be defined the inverter to stop (by the inverter index)
                var data = new InverterStopFieldMessageData( InverterIndex.MainInverter );

                var stopMessage = new FieldCommandMessage(
                    data,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop );

                this.ParentStateMachine.PublishFieldCommandMessage( stopMessage );

                var notificationMessage = new NotificationMessage(
                    this.positioningMessageData,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationStop );

                this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
            }
            else
            {
                var notificationMessage = new NotificationMessage(
                    this.positioningMessageData,
                    this.positioningMessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationEnd );

                this.Logger.LogDebug( "FSM Positioning End" );

                this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
            }
        }

        public override void Stop()
        {
            this.Logger.LogTrace( "1:Method Start" );
        }

        protected override void Dispose( bool disposing )
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
