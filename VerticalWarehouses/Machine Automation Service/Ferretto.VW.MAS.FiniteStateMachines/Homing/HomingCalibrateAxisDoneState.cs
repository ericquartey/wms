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
    internal class HomingCalibrateAxisDoneState : StateBase
    {

        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IHomingStateData stateData;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public HomingCalibrateAxisDoneState(IHomingStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
        }

        #endregion

        #region Destructors

        ~HomingCalibrateAxisDoneState()
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

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData));
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
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                if (this.machineData.NumberOfExecutedSteps == this.machineData.MaximumSteps)
                {
                    this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData));
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.stateData));
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (this.machineData.IsOneKMachine)
            {
                this.ioSwitched = true;
                var slaveInverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.machineData.AxisToCalibrate);
                var slaveInverterCommandMessage = new FieldCommandMessage(
                    slaveInverterCommandMessageData,
                    $"Switch Axis {this.machineData.AxisToCalibrate}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterSwitchOn,
                    (byte)(this.machineData.AxisToCalibrate == Axis.Horizontal ? InverterIndex.Slave1 : InverterIndex.MainInverter));

                this.Logger.LogTrace($"5:Publishing Field Command Message {slaveInverterCommandMessage.Type} Destination {slaveInverterCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(slaveInverterCommandMessage);
            }
            else
            {
                var ioCommandMessageData = new SwitchAxisFieldMessageData(this.machineData.AxisToCalibrate);
                var ioCommandMessage = new FieldCommandMessage(
                    ioCommandMessageData,
                    $"Switch Axis {this.machineData.AxisToCalibrate}",
                    FieldMessageActor.IoDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.SwitchAxis,
                    (byte)IoIndex.IoDevice1);

                this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.machineData.AxisToCalibrate);
                var inverterCommandMessage = new FieldCommandMessage(
                    inverterCommandMessageData,
                    $"Switch Axis {this.machineData.AxisToCalibrate}",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterSwitchOn,
                    (byte)InverterIndex.MainInverter);

                this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

                this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);
            }

            var notificationMessageData = new CalibrateAxisMessageData(this.machineData.AxisToCalibrated, this.machineData.NumberOfExecutedSteps, this.machineData.MaximumSteps, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.machineData.AxisToCalibrated} axis calibration completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationEnd);

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
