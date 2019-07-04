using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class PositioningStartState : StateBase
    {
        #region Fields

        private readonly IPositioningMessageData positioningMessageData;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public PositioningStartState(
            IStateMachine parentMachine,
            IPositioningMessageData positioningMessageData,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.positioningMessageData = positioningMessageData;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PositioningStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.positioningMessageData, message, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new PositioningExecutingState(this.ParentStateMachine, this.positioningMessageData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var ioCommandMessageData = new SwitchAxisFieldMessageData(this.positioningMessageData.AxisMovement);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Switch Axis {this.positioningMessageData.AxisMovement}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            //TODO Check if hard coding inverter index on MainInverter is correct or a dynamic selection of inverter index is required
            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.positioningMessageData.AxisMovement, InverterIndex.MainInverter);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {this.positioningMessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            var notificationMessage = new NotificationMessage(
                this.positioningMessageData,
                this.positioningMessageData.NumberCycles == 0 ? $"{this.positioningMessageData.AxisMovement} Positioning Started" : "Burnishing Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningEndState(this.ParentStateMachine, this.positioningMessageData, this.Logger, 0, true));
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
