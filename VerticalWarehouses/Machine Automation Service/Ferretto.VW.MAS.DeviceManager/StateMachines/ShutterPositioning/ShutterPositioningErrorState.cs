using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning
{
    internal class ShutterPositioningErrorState : StateBase
    {
        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IShutterPositioningStateData stateData;

        #endregion

        #region Constructors

        public ShutterPositioningErrorState(IShutterPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.InverterStop && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
                var inverterStatus = new AglInverterStatus((InverterIndex)message.DeviceIndex, this.ParentStateMachine.ServiceScopeFactory);
                var sensorStart = (int)(IOMachineSensors.PowerOnOff + message.DeviceIndex * inverterStatus.Inputs.Length);
                Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);
                notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Shutter Positioning Stopped for an error",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.ShutterPositioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.InverterIndex}");

            var stopMessage = new FieldCommandMessage(
                null,
                "Reset Inverter ShutterPositioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterStop,
                (byte)this.machineData.InverterIndex);

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest)
            {
                //this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                //this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");

                switch (this.machineData.TargetBay)
                {
                    case BayNumber.BayOne:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                        break;

                    case BayNumber.BayTwo:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual2;
                        break;

                    case BayNumber.BayThree:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual3;
                        break;

                    default:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                        break;
                }

                this.Logger.LogInformation($"Machine status switched to {this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode}");
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = StopRequestReason.Stop;
            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
