using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningStartState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IPositioningStateData stateData;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public PositioningStartState(IPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status} Axis:{this.machineData.MessageData.AxisMovement}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.Logger.LogDebug("I/O switch completed");
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceError, this.machineData.RequestingBay);
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        this.Logger.LogDebug("Inverter switch ON completed");
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOff)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.Logger.LogDebug("Inverter switch OFF completed");
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new PositioningExecutingState(this.stateData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex} Axis:{this.machineData.MessageData.AxisMovement}");
            if (!this.machineData.MessageData.IsOneTonMachine &&
                this.machineData.MessageData.MovementMode < MovementMode.ShutterPosition)
            {
                var ioCommandMessageData = new SwitchAxisFieldMessageData(this.machineData.MessageData.AxisMovement);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Switch Axis {this.machineData.MessageData.AxisMovement}",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.SwitchAxis,
                    (byte)IoIndex.IoDevice1);

                this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);
            }
            else
            {
                this.ioSwitched = true;
            }

            {
                // Send a field message to the Update of position axis to InverterDriver
                var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 250);
                var inverterMessage = new FieldCommandMessage(
                    inverterDataMessage,
                    "Update Inverter digital input status",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterSetTimer,
                    (byte)InverterIndex.MainInverter);

                this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            }
            var inverterIndex = this.machineData.CurrentInverterIndex;

            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.machineData.MessageData.AxisMovement);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {this.machineData.MessageData.AxisMovement}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSwitchOn,
                (byte)inverterIndex);

            this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                if (this.machineData.MessageData.MovementMode == MovementMode.BeltBurnishing)
                {
                    this.machineData.MessageData.ExecutedCycles = scope.ServiceProvider
                        .GetRequiredService<ISetupProceduresDataProvider>()
                        .GetBeltBurnishingTest()
                        .PerformedCycles;

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
                }
                else if (this.machineData.MessageData.MovementMode == MovementMode.BayTest)
                {
                    this.machineData.MessageData.ExecutedCycles = scope.ServiceProvider
                        .GetRequiredService<ISetupProceduresDataProvider>()
                        .GetBayCarouselCalibration(this.machineData.RequestingBay)
                        .PerformedCycles;

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
                }
            }

            this.Logger.LogTrace(
                $"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement};");

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                this.machineData.MessageData.RequiredCycles == 0
                    ? $"{this.machineData.MessageData.AxisMovement} Positioning Started"
                    : "Burnishing Started",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method: Start. Reason:{reason} Axis:{this.machineData.MessageData.AxisMovement}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
