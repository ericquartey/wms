using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
using System.Threading;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    internal class ShutterPositioningExecutingState : StateBase
    {
        #region Fields

        private readonly Timer delayTimer;

        private readonly IShutterPositioningMachineData machineData;

        private readonly IShutterPositioningStateData stateData;

        private bool disposed;

        private int numberOfExecutedCycles;

        private ShutterMovementDirection oldDirection;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(IShutterPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
            this.delayTimer = new Timer(this.DelayTimerMethod, null, -1, Timeout.Infinite);
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
                            if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest)
                            {
                                if (messageData.ShutterPosition == ShutterPosition.Opened)
                                {
                                    this.numberOfExecutedCycles++;
                                    if (this.numberOfExecutedCycles == this.machineData.PositioningMessageData.RequestedCycles)
                                    {
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
                                    }
                                    else
                                    {
                                        this.StartPositioning(messageData.ShutterPosition, ShutterMovementDirection.Down);
                                    }
                                }
                                else if (messageData.ShutterPosition == ShutterPosition.Closed)
                                {
                                    if (this.machineData.PositioningMessageData.Delay > 0)
                                    {
                                        this.delayTimer.Change(this.machineData.PositioningMessageData.Delay, this.machineData.PositioningMessageData.Delay);
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
                                    this.stateData.FieldMessage = message;
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
                            }
                            break;

                        case MessageStatus.OperationError:
                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                            break;
                    }
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.machineData.InverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus.Inputs.Length);
            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Move from {notificationMessageData.ShutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.machineData.RequestingBay,
                this.machineData.RequestingBay,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.oldDirection = ShutterMovementDirection.Down;
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            // stop timer
            this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void DelayTimerMethod(object state)
        {
            // stop timer
            this.delayTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // delay expired
            this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
        }

        private void StartPositioning(ShutterPosition position, ShutterMovementDirection direction)
        {
            ShutterPosition shutterPositionTarget;
            if (direction == ShutterMovementDirection.Down)
            {
                shutterPositionTarget = ShutterPosition.Closed;
                if (this.machineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Opened)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            else
            {
                shutterPositionTarget = ShutterPosition.Opened;
                if (this.machineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type && position == ShutterPosition.Closed)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            // speed is negative to go up
            var speedRate = this.machineData.PositioningMessageData.SpeedRate * ((direction == ShutterMovementDirection.Up) ? -1 : 1);

            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                direction,
                this.machineData.PositioningMessageData.ShutterType,
                speedRate,
                this.machineData.PositioningMessageData.HigherDistance,
                this.machineData.PositioningMessageData.LowerDistance,
                this.machineData.PositioningMessageData.HighSpeedPercent,
                this.machineData.PositioningMessageData.LowerSpeed,
                this.machineData.PositioningMessageData.MovementType);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.machineData.InverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.machineData.PositioningMessageData.ExecutedCycles = this.numberOfExecutedCycles;
            var notificationMessage = new NotificationMessage(
                this.machineData.PositioningMessageData,
                "ShutterControl Test Executing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.machineData.RequestingBay,
                this.machineData.RequestingBay,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.oldDirection = direction;
        }

        #endregion
    }
}
