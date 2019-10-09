using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    internal class HomingEndState : StateBase
    {
        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IHomingStateData stateData;

        private bool isDisposed;

        #endregion

        #region Constructors

        public HomingEndState(IHomingStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
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
                case FieldMessageType.InverterPowerOff:
                case FieldMessageType.CalibrateAxis:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            break;

                        case MessageStatus.OperationError:
                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData));
                            break;
                    }
                    break;
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
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.machineData.IsOneKMachine)
            {
                inverterMessage = new FieldCommandMessage(
                    inverterDataMessage,
                    "Update Inverter axis position status",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterSetTimer,
                    (byte)InverterIndex.Slave1);

                this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            }

            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var targetInverter = (this.machineData.IsOneKMachine && this.machineData.AxisToCalibrate == Axis.Horizontal) ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                var stopMessage = new FieldCommandMessage(
                    null,
                    "Homing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                    (byte)targetInverter);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }

            var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, this.machineData.CalibrationType, MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Completed",
                MessageActor.FiniteStateMachines,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            if (this.stateData.StopRequestReason == StopRequestReason.NoReason
                &&
                this.machineData.AxisToCalibrate != Axis.BayChain)
            {
                var setupStatusProvider = this.scope.ServiceProvider.GetRequiredService<ISetupStatusProvider>();

                setupStatusProvider.CompleteVerticalOrigin();
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.scope.Dispose();
            }

            base.Dispose(disposing);

            this.isDisposed = true;
        }

        #endregion
    }
}
