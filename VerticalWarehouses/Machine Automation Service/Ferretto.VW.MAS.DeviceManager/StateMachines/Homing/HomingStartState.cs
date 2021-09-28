using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingStartState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IHomingMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IHomingStateData stateData;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public HomingStartState(IHomingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
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
            this.Logger.LogTrace($"{this.GetType().Name} ProcessFieldNotificationMessage: type: {message.Type}, status{message.Status}");

            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.IoDeviceError, this.machineData.RequestingBay);
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData, this.Logger));
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
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.stateData, this.Logger));
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}, axis {this.machineData.AxisToCalibrate}, calibration {this.machineData.CalibrationType}, turnBack {this.machineData.TurnBack}");

            if (this.machineData.IsOneTonMachine && this.machineData.AxisToCalibrate == Axis.Horizontal)
            {
                this.machineData.CurrentInverterIndex = InverterIndex.Slave1;
            }
            var inverterIndex = this.machineData.CurrentInverterIndex;

            if (!this.machineData.IsOneTonMachine && this.machineData.AxisToCalibrate != Axis.BayChain)
            {
                var ioCommandMessageData = new SwitchAxisFieldMessageData(this.machineData.AxisToCalibrate);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Switch Axis {this.machineData.AxisToCalibrate}",
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

            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.machineData.AxisToCalibrate);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {this.machineData.AxisToCalibrate}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSwitchOn,
                (byte)inverterIndex);

            this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var notificationMessageData = new HomingMessageData(this.machineData.RequestedAxisToCalibrate, this.machineData.CalibrationType, this.machineData.LoadingUnitId, false, false, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Started",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.Homing,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("{type} Stop: {reason}", this.GetType().Name, reason);

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
