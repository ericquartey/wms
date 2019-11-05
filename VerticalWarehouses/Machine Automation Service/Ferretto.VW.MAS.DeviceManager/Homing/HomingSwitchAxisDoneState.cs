using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IHomingStateData stateData;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(IHomingStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
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
                        this.ParentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData));
                        break;
                }
            }
            else if (message.Type == FieldMessageType.InverterStatusUpdate)
            {
                if (message.Data is IInverterStatusUpdateFieldMessageData dataInverters)
                {
                    if (dataInverters.CurrentPosition != null)
                    {
                        var notificationData = new PositioningMessageData();
                        notificationData.CurrentPosition = dataInverters.CurrentPosition.Value;
                        notificationData.AxisMovement = dataInverters.CurrentAxis;
                        if (this.machineData.AxisToCalibrate == Axis.BayChain)
                        {
                            notificationData.MovementMode = MovementMode.BayChain;
                        }
                        this.Logger.LogTrace($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={notificationData.AxisMovement}; value={(int)dataInverters.CurrentPosition.Value}");
                        var notificationMessage = new NotificationMessage(
                            notificationData,
                            $"Current Encoder position: {notificationData.CurrentPosition}",
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
            this.Logger.LogDebug($"Start {this.GetType().Name}");

            var inverterIndex = this.machineData.CurrentInverterIndex;

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
            this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData));
        }

        #endregion
    }
}
