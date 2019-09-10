using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
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

        private readonly Timer delayTimer;

        private readonly InverterIndex inverterIndex;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly int numberOfRequestedCycles;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        private ShutterMovementDirection OldDirection;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(
            IStateMachine parentMachine,
            IShutterPositioningMessageData shutterPositioningMessageData,
            InverterIndex inverterIndex,
            IMachineSensorsStatus machineSensorsStatus,
            Timer delayTimer,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.shutterPositioningMessageData = shutterPositioningMessageData;
            this.numberOfRequestedCycles = shutterPositioningMessageData.RequestedCycles;
            this.machineSensorsStatus = machineSensorsStatus;
            this.delayTimer = delayTimer;
            this.inverterIndex = inverterIndex;
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
                            if (this.shutterPositioningMessageData.MovementMode == MovementMode.TestLoop)
                            {
                                if (messageData.ShutterPosition == ShutterPosition.Opened)
                                {
                                    this.shutterPositioningMessageData.ExecutedCycles++;
                                    if (this.shutterPositioningMessageData.ExecutedCycles == this.numberOfRequestedCycles)
                                    {
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger));
                                    }
                                    else
                                    {
                                        this.StartPositioning(messageData.ShutterPosition, ShutterMovementDirection.Down);
                                    }
                                }
                                else if (messageData.ShutterPosition == ShutterPosition.Closed)
                                {
                                    if (this.shutterPositioningMessageData.Delay > 0)
                                    {
                                        this.delayTimer.Change(this.shutterPositioningMessageData.Delay, this.shutterPositioningMessageData.Delay);
                                    }
                                    else
                                    {
                                        this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
                                    }
                                }
                                else if (messageData.ShutterPosition == ShutterPosition.Half)
                                {
                                    this.StartPositioning(messageData.ShutterPosition, this.OldDirection);
                                }
                                else
                                {
                                    this.Logger.LogError($"Invalid position of Shutter at Operation End: {messageData.ShutterPosition}");
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger));
                            }
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
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
            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.inverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.inverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Move from {notificationMessageData.ShutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.OldDirection = ShutterMovementDirection.Down;
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            // stop timer
            this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger, true));
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
                if (this.shutterPositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Opened)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            else
            {
                shutterPositionTarget = ShutterPosition.Opened;
                if (this.shutterPositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Closed)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                direction,
                this.shutterPositioningMessageData.ShutterType,
                this.shutterPositioningMessageData.SpeedRate,
                this.shutterPositioningMessageData.HigherDistance,
                this.shutterPositioningMessageData.LowerDistance,
                this.shutterPositioningMessageData.MovementType);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.inverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.shutterPositioningMessageData,
                "Shutter Positioning Executing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.OldDirection = direction;
        }

        #endregion
    }
}
