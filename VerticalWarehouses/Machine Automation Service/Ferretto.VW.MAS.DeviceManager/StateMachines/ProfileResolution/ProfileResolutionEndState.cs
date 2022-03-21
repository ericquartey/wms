using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ProfileResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.ProfileResolution
{
    internal class ProfileResolutionEndState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IProfileResolutionMachineData machineData;

        //private readonly double minHeight = 25.0;

        private readonly IServiceScope scope;

        private readonly IProfileResolutionStateData stateData;

        #endregion

        #region Constructors

        public ProfileResolutionEndState(IProfileResolutionStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IProfileResolutionMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                case FieldMessageType.Positioning:
                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterSwitchOff:
                    if (message.DeviceIndex != (byte)this.machineData.CurrentInverterIndex)
                    {
                        break;
                    }
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:

                            var notificationMessage = new NotificationMessage(
                                this.machineData.MessageData,
                                "Profile Resolution Stopped",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                StopRequestReasonConverter.GetMessageStatusFromReason(StopRequestReason.Stop));

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.TargetBay);
                            this.ParentStateMachine.ChangeState(new ProfileResolutionErrorState(this.stateData, this.Logger));
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex} StopRequestReason {this.stateData.StopRequestReason}");

            //if (this.machineData.MessageData.AxisMovement is Axis.Vertical)
            //{
            //    this.PersistElevatorPosition(
            //        this.machineData.MessageData.TargetBayPositionId,
            //        this.machineData.MessageData.TargetCellId,
            //        this.machineData.MessageData.TargetPosition);
            //}

            var inverterIndex = this.machineData.CurrentInverterIndex;
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    "Profile Resolution Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterStop,
                    (byte)inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                if (this.machineData.MessageData.AxisMovement is Axis.Vertical)
                {
                    this.UpdateLastIdealPosition(this.machineData.MessageData.AxisMovement);
                }

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    "Profile Resolution Completed",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Profile Resolution End");
            }

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
            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.machineData.MessageData.MovementMode == MovementMode.ProfileResolution)
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
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        private void UpdateLastIdealPosition(Axis axis)
        {
            var serviceProvider = this.ParentStateMachine.ServiceScopeFactory.CreateScope().ServiceProvider;
            if (axis == Axis.Vertical)
            {
                var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
                elevatorDataProvider.UpdateLastIdealPosition(this.machineData.MessageData.TargetPosition);
            }
        }

        #endregion
    }
}
