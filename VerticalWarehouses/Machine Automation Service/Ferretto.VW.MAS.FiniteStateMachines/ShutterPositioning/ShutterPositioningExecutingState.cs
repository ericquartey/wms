using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningExecutingState : StateBase
    {

        #region Fields

        private readonly IShutterPositioningStateMachineData shutterPositioningStateMachineData;

        private bool disposed;

        private int numberOfExecutedCycles;

        private ShutterMovementDirection oldDirection;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(
            IStateMachine parentMachine,
            IShutterPositioningStateMachineData shutterPositioningStateMachineData)
            : base(parentMachine, shutterPositioningStateMachineData.Logger)
        {
            this.shutterPositioningStateMachineData = shutterPositioningStateMachineData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningExecutingState()
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                if (message.Data is InverterShutterPositioningFieldMessageData messageData)
                {
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            if (this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.TestLoop)
                            {
                                if (messageData.ShutterPosition == ShutterPosition.Opened)
                                {
                                    this.numberOfExecutedCycles++;
                                    if (this.numberOfExecutedCycles == this.shutterPositioningStateMachineData.PositioningMessageData.RequestedCycles)
                                    {
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData));
                                    }
                                    else
                                    {
                                        this.StartPositioning(messageData.ShutterPosition, ShutterMovementDirection.Down);
                                    }
                                }
                                else if (messageData.ShutterPosition == ShutterPosition.Closed)
                                {
                                    if (this.shutterPositioningStateMachineData.PositioningMessageData.Delay > 0)
                                    {
                                        this.shutterPositioningStateMachineData.DelayTimer.Change(this.shutterPositioningStateMachineData.PositioningMessageData.Delay, this.shutterPositioningStateMachineData.PositioningMessageData.Delay);
                                    }
                                    else
                                    {
                                        this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
                                    }
                                }
                                else if (messageData.ShutterPosition == ShutterPosition.Half)
                                {
                                    this.StartPositioning(messageData.ShutterPosition, this.oldDirection);
                                }
                                else
                                {
                                    this.Logger.LogError($"Invalid position of Shutter at Operation End: {messageData.ShutterPosition}");
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningStateMachineData, message));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData));
                            }
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningStateMachineData, message));
                            break;
                    }
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
            if (message.Type == MessageType.CheckCondition &&
                message.Status == MessageStatus.OperationExecuting)
            {
                // delay expired
                this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
            }
        }

        public override void Start()
        {
            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningStateMachineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.shutterPositioningStateMachineData.InverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.shutterPositioningStateMachineData.InverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.shutterPositioningStateMachineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Move from {notificationMessageData.ShutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.shutterPositioningStateMachineData.RequestingBay,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.oldDirection = ShutterMovementDirection.Down;
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData, true));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void StartPositioning(ShutterPosition position, ShutterMovementDirection direction)
        {
            ShutterPosition shutterPositionTarget;
            if (direction == ShutterMovementDirection.Down)
            {
                shutterPositionTarget = ShutterPosition.Closed;
                if (this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Opened)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            else
            {
                shutterPositionTarget = ShutterPosition.Opened;
                if (this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Closed)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                direction,
                this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType,
                this.shutterPositioningStateMachineData.PositioningMessageData.SpeedRate);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.shutterPositioningStateMachineData.InverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.shutterPositioningStateMachineData.PositioningMessageData,
                "ShutterControl Test Executing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.shutterPositioningStateMachineData.RequestingBay,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.oldDirection = direction;
        }

        #endregion
    }
}
