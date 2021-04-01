using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingCalibrateAxisDoneState : StateBase
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

        public HomingCalibrateAxisDoneState(IHomingStateData stateData, ILogger logger)
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
            else if (message.Type == FieldMessageType.InverterSwitchOff)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.Logger.LogDebug("Inverter switch OFF completed");
                        break;

                    case MessageStatus.OperationError:
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
                if (this.machineData.NumberOfExecutedSteps == this.machineData.MaximumSteps)
                {
                    this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.stateData, this.Logger));
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        /// <inheritdoc/>
        public override void Start()
        {
            var inverterIndex = this.machineData.CurrentInverterIndex;
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {inverterIndex}");

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

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            if (this.machineData.AxisToCalibrate == Axis.Horizontal)
            {
                var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                elevatorDataProvider.UpdateLastIdealPosition(0);
                elevatorDataProvider.UpdateLastCalibrationCycles();
            }
            else if (this.machineData.AxisToCalibrate == Axis.Vertical)
            {
                var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                var machineVolatileDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                elevatorDataProvider.UpdateLastIdealPosition(machineVolatileDataProvider.ElevatorVerticalPosition, Orientation.Vertical);
            }

            var notificationMessageData = new CalibrateAxisMessageData(this.machineData.AxisToCalibrate, this.machineData.NumberOfExecutedSteps, this.machineData.MaximumSteps, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.machineData.InverterIndexOld} axis calibration completed",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.CalibrateAxis,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStepStop);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
