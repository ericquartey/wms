using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IHomingMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IHomingStateData stateData;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(IHomingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"{this.GetType().Name} ProcessFieldNotificationMessage: type: {message.Type}, status{message.Status}");

            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.stateData, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.TargetBay);
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
            else if (message.Type == FieldMessageType.InverterStatusUpdate)
            {
                if (message.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                {
                    if (dataInverters.CurrentPosition != null)
                    {
                        var notificationData = new PositioningMessageData
                        {
                            AxisMovement = dataInverters.CurrentAxis
                        };

                        if (this.machineData.AxisToCalibrate == Axis.BayChain)
                        {
                            notificationData.MovementMode = MovementMode.BayChain;
                        }

                        this.Logger.LogTrace($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={notificationData.AxisMovement}; value={(int)dataInverters.CurrentPosition.Value}");

                        var notificationMessage = new NotificationMessage(
                            notificationData,
                            $"Current Encoder position: {dataInverters.CurrentPosition.Value}",
                            MessageActor.AutomationService,
                            MessageActor.DeviceManager,
                            MessageType.Positioning,
                            this.machineData.RequestingBay,
                            this.machineData.TargetBay,
                            MessageStatus.OperationExecuting);

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = this.machineData.CurrentInverterIndex;
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {inverterIndex}");

            if (this.machineData.AxisToCalibrate == Axis.Vertical)
            {
                this.machineData.VerticalStartingPosition = this.elevatorProvider.VerticalPosition;
                // check again the conditions
                if (!(this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    && !(this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    )
                {
                    if (this.machineData.ShowErrors)
                    {
                        this.errorsProvider.RecordNew(DataModels.MachineErrorCode.ConditionsNotMetForHoming, this.machineData.RequestingBay);
                    }

                    this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData, this.Logger));
                }
            }

            var calibrateAxisData = new CalibrateAxisFieldMessageData(this.machineData.AxisToCalibrate, this.machineData.CalibrationType);
            var commandMessage = new FieldCommandMessage(
                calibrateAxisData,
                $"Homing {this.machineData.AxisToCalibrate} State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.CalibrateAxis,
                (byte)inverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new CalibrateAxisMessageData(this.machineData.AxisToCalibrate, this.machineData.NumberOfExecutedSteps + 1, this.machineData.MaximumSteps, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.machineData.AxisToCalibrate} axis calibration started",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.CalibrateAxis,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStepStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"{this.GetType().Name} Stop: {reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
