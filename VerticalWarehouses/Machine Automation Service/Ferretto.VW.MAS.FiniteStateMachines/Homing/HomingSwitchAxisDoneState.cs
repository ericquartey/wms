using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
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

        #region Destructors

        ~HomingSwitchAxisDoneState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
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
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = (this.machineData.IsOneKMachine && this.machineData.AxisToCalibrate == Axis.Horizontal)
                ? InverterIndex.Slave1
                : InverterIndex.MainInverter;

            var calibrateAxisData = new CalibrateAxisFieldMessageData(this.machineData.AxisToCalibrate);
            var commandMessage = new FieldCommandMessage(
                calibrateAxisData,
                $"Homing {this.machineData.AxisToCalibrate} State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.CalibrateAxis,
                (byte)inverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new CalibrateAxisMessageData(this.machineData.AxisToCalibrate, this.machineData.NumberOfExecutedSteps + 1, this.machineData.MaximumSteps, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.machineData.AxisToCalibrate} axis calibration started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData));
        }

        #endregion
    }
}
