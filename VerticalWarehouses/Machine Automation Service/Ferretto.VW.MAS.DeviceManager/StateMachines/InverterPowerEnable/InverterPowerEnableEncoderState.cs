using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable
{
    internal class InverterPowerEnableEncoderState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IInverterPowerEnableMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IInverterPowerEnableStateData stateData;

        private bool inverterSwitched;

        private bool ioSwitched;

        private DateTime startTime;

        #endregion

        #region Constructors

        public InverterPowerEnableEncoderState(IInverterPowerEnableStateData stateData, ILogger logger)
                    : base(stateData?.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData?.MachineData as IInverterPowerEnableMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.startTime = DateTime.UtcNow;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"{this.GetType().Name} ProcessFieldNotificationMessage: type: {message.Type}, status: {message.Status}, inverter {message.DeviceIndex}");
            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        this.Logger.LogDebug("SwitchAxis completed");
                        break;

                    case MessageStatus.OperationError:
                        //this.errorsProvider.RecordNew(MachineErrorCode.IoDeviceError, this.machineData.RequestingBay);
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterPowerEnableErrorState(this.stateData, this.Logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        this.startTime = DateTime.UtcNow;
                        this.Logger.LogDebug("Inverter switch ON completed");
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterPowerEnableErrorState(this.stateData, this.Logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterSwitchOff)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new InverterPowerEnableErrorState(this.stateData, this.Logger));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterStatusUpdate
                && message.Status == MessageStatus.OperationExecuting
                && this.inverterSwitched
                && DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > 500
                && message.Data is InverterStatusUpdateFieldMessageData data
                )
            {
                if (data.CurrentAxis == Axis.Horizontal)
                {
                    this.Logger.LogDebug("Horizontal position received");
                    var axis = Axis.Vertical;
                    var ioCommandMessageData = new SwitchAxisFieldMessageData(Axis.Horizontal);
                    var ioCommandMessage = new FieldCommandMessage(
                        ioCommandMessageData,
                        $"Switch Axis {axis}",
                        FieldMessageActor.IoDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.SwitchAxis,
                        (byte)IoIndex.IoDevice1);

                    this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                    var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(axis);
                    var inverterCommandMessage = new FieldCommandMessage(
                        inverterCommandMessageData,
                        $"Switch Axis {axis}",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.InverterSwitchOn,
                        (byte)InverterIndex.MainInverter);

                    this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

                    this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

                    this.ioSwitched = false;
                    this.inverterSwitched = false;
                }
                else if (data.CurrentAxis == Axis.Vertical)
                {
                    this.Logger.LogDebug("Vertical position received");
                    var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
                    var inverterMessage = new FieldCommandMessage(
                        inverterDataMessage,
                        "Update Inverter timer",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.DeviceManager,
                        FieldMessageType.InverterSetTimer,
                        (byte)InverterIndex.MainInverter);

                    this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

                    this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

                    var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                    if (machineProvider.ElevatorVerticalPositionOld != -10000 &&
                        Math.Abs(machineProvider.ElevatorVerticalPosition - machineProvider.ElevatorVerticalPositionOld) > 10)
                    {
                        this.Logger.LogWarning($"Vertical position changed after machine start: old value {machineProvider.ElevatorVerticalPositionOld}, new value {machineProvider.ElevatorVerticalPosition}");
                        //machineProvider.IsHomingExecuted = false;
                        //this.stateData.FieldMessage = message;
                        //this.errorsProvider.RecordNew(MachineErrorCode.VerticalPositionChanged, this.machineData.RequestingBay);
                        //this.ParentStateMachine.ChangeState(new InverterPowerEnableErrorState(this.stateData));
                    }
                    //else
                    {
                        this.ParentStateMachine.ChangeState(new InverterPowerEnableEndState(this.stateData, this.Logger));
                    }
                    return;
                }
            }

            if (this.ioSwitched
                && this.inverterSwitched
                )
            {
                var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, true, 200);
                var inverterMessage = new FieldCommandMessage(
                    inverterDataMessage,
                    "Update Inverter timer",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterSetTimer,
                    (byte)InverterIndex.MainInverter);

                this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            this.startTime = DateTime.UtcNow;
            this.ioSwitched = false;
            this.inverterSwitched = false;
            var axis = Axis.Horizontal;
            this.Logger.LogDebug($"Start with requested state: {this.machineData.Enable} Bay: {this.machineData.TargetBay}");
            var ioCommandMessageData = new SwitchAxisFieldMessageData(Axis.Horizontal);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Switch Axis {axis}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.SwitchAxis,
                (byte)IoIndex.IoDevice1);

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(axis);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {axis}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSwitchOn,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new InverterPowerEnableEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
