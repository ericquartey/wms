using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning
{
    internal class ShutterPositioningExecutingState : StateBase, IDisposable
    {
        #region Fields

        private readonly Timer delayTimerDown;

        private readonly Timer delayTimerUp;

        private readonly IShutterPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IShutterPositioningStateData stateData;

        private bool disposed;

        private ShutterMovementDirection oldDirection;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(IShutterPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
            this.delayTimerDown = new Timer(this.DelayTimerMethodDown, null, -1, Timeout.Infinite);
            this.delayTimerUp = new Timer(this.DelayTimerMethodUp, null, -1, Timeout.Infinite);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning
                &&
                message.Data is InverterShutterPositioningFieldMessageData messageData)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest)
                        {
                            this.OnShutterTestStageCompleted(message, messageData);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData, this.Logger));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.InverterIndex}");

            var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus(this.machineData.InverterIndex, this.ParentStateMachine.ServiceScopeFactory);
            var sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus.Inputs.Length);
            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Move from {notificationMessageData.ShutterPosition}",
                MessageActor.Any,
                MessageActor.DeviceManager,
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
            this.delayTimerDown.Change(Timeout.Infinite, Timeout.Infinite);
            this.delayTimerUp.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData, this.Logger));
        }

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimerDown?.Dispose();
                this.delayTimerUp?.Dispose();
                this.scope?.Dispose();
            }

            this.disposed = true;
        }

        private void DelayTimerMethodDown(object state)
        {
            // stop timer
            this.delayTimerDown.Change(Timeout.Infinite, Timeout.Infinite);

            // delay expired
            this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
        }

        private void DelayTimerMethodUp(object state)
        {
            // stop timer
            this.delayTimerUp.Change(Timeout.Infinite, Timeout.Infinite);

            // delay expired
            this.StartPositioning(ShutterPosition.Opened, ShutterMovementDirection.Down);
        }

        private void OnShutterTestStageCompleted(FieldNotificationMessage message, InverterShutterPositioningFieldMessageData messageData)
        {
            switch (this.machineData.TargetBay)
            {
                case BayNumber.BayOne:
                    this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Test;
                    break;

                case BayNumber.BayTwo:
                    this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Test2;
                    break;

                case BayNumber.BayThree:
                    this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Test3;
                    break;

                default:
                    this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Test;
                    break;
            }

            this.Logger.LogInformation($"Machine status switched to {this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode}");
            switch (messageData.ShutterPosition)
            {
                case ShutterPosition.Opened:
                    if (this.machineData.PositioningMessageData.Delay > 0)
                    {
                        this.delayTimerUp.Change(this.machineData.PositioningMessageData.Delay, this.machineData.PositioningMessageData.Delay);
                    }
                    else
                    {
                        this.StartPositioning(ShutterPosition.Opened, ShutterMovementDirection.Down);
                    }

                    break;

                case ShutterPosition.Closed:
                    var setupProceduresDataProvider = this.ParentStateMachine.ServiceScopeFactory
                        .CreateScope()
                        .ServiceProvider
                        .GetRequiredService<ISetupProceduresDataProvider>();

                    var testParameters = setupProceduresDataProvider.IncreasePerformedCycles(
                        setupProceduresDataProvider.GetBayShutterTest(this.machineData.RequestingBay));

                    this.machineData.PositioningMessageData.PerformedCycles = testParameters.PerformedCycles;

                    if (testParameters.PerformedCycles >= testParameters.RequiredCycles)
                    {
                        setupProceduresDataProvider.MarkAsCompleted(setupProceduresDataProvider.GetBayShutterTest(this.machineData.RequestingBay));
                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData, this.Logger));
                    }
                    else
                    {
                        if (this.machineData.PositioningMessageData.Delay > 0)
                        {
                            this.delayTimerDown.Change(this.machineData.PositioningMessageData.Delay, this.machineData.PositioningMessageData.Delay);
                        }
                        else
                        {
                            this.StartPositioning(ShutterPosition.Closed, ShutterMovementDirection.Up);
                        }
                    }

                    break;

                case ShutterPosition.Half:
                    this.StartPositioning(messageData.ShutterPosition, this.oldDirection);
                    break;

                default:
                    this.Logger.LogError($"Invalid position of Shutter at Operation End: {messageData.ShutterPosition}");
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData, this.Logger));
                    break;
            }
        }

        private void StartPositioning(ShutterPosition position, ShutterMovementDirection direction)
        {
            ShutterPosition shutterPositionTarget;
            if (direction == ShutterMovementDirection.Down)
            {
                shutterPositionTarget = ShutterPosition.Closed;
                if (this.machineData.PositioningMessageData.ShutterType == ShutterType.ThreeSensors
                    &&
                    position == ShutterPosition.Opened)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }
            else
            {
                shutterPositionTarget = ShutterPosition.Opened;
                if (this.machineData.PositioningMessageData.ShutterType == ShutterType.ThreeSensors
                    &&
                    position == ShutterPosition.Closed)
                {
                    shutterPositionTarget = ShutterPosition.Half;
                }
            }

            // speed is negative to go up
            var directionMultiplier = direction == ShutterMovementDirection.Up ? -1 : 1;

            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                direction,
                this.machineData.PositioningMessageData.ShutterType,
                this.machineData.PositioningMessageData.SpeedRate * directionMultiplier,
                this.machineData.PositioningMessageData.HighSpeedDurationOpen,
                this.machineData.PositioningMessageData.HighSpeedDurationClose,
                this.machineData.PositioningMessageData.HighSpeedHalfDurationOpen,
                this.machineData.PositioningMessageData.HighSpeedHalfDurationClose,
                this.machineData.PositioningMessageData.LowerSpeed * directionMultiplier,
                this.machineData.PositioningMessageData.MovementType);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ShutterPositioning,
                (byte)this.machineData.InverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.machineData.PositioningMessageData,
                "ShutterControl Test Executing",
                MessageActor.Any,
                MessageActor.DeviceManager,
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
