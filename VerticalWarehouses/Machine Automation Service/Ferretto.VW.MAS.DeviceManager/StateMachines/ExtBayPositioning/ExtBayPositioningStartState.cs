using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ExtBayPositioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.StateMachines.ExtBayPositioning;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ExtBayPositioning
{
    internal class ExtBayPositioningStartState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IExtBayPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IExtBayPositioningStateData stateData;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public ExtBayPositioningStartState(IExtBayPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IExtBayPositioningMachineData;
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
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        this.Logger.LogDebug("Inverter switch ON completed");
                        break;

                    case MessageStatus.OperationError:
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.RequestingBay);
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new ExtBayPositioningErrorState(this.stateData, this.Logger));
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
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.RequestingBay);
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new ExtBayPositioningErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new ExtBayPositioningExecutingState(this.stateData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex}");

            this.ioSwitched = true;

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
                if (this.machineData.MessageData.MovementMode == MovementMode.ExtBayTest)
                {
                    this.machineData.MessageData.ExecutedCycles = scope.ServiceProvider
                        .GetRequiredService<ISetupProceduresDataProvider>()
                        .GetBayExternalCalibration(this.machineData.RequestingBay)
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
                    ? $"{this.machineData.MessageData.AxisMovement} External Bay Positioning Started"
                    : "External Bay Calibration Test Started",
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
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
