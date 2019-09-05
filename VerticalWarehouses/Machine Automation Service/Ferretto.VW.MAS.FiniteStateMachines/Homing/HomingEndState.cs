using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    public class HomingEndState : StateBase
    {

        #region Fields

        private readonly IHomingMachineData machineData;

        private readonly IHomingStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingEndState(IHomingStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.RequestingBay, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;
        }

        #endregion

        #region Destructors

        ~HomingEndState()
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

            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                if (this.machineData.AxisToCalibrate == Axis.Vertical)
                {
                    var stopMessage = new FieldCommandMessage(
                        null,
                        "Homing Stopped",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop,
                        (byte)InverterIndex.MainInverter);

                    this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
                }

                if (this.machineData.IsOneKMachine && this.machineData.AxisToCalibrate == Axis.Horizontal)
                {
                    var stopMessage = new FieldCommandMessage(
                        null,
                        "Homing Stopped",
                        FieldMessageActor.InverterDriver,
                        FieldMessageActor.FiniteStateMachines,
                        FieldMessageType.InverterStop,
                        (byte)InverterIndex.Slave1);

                    this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
                }
            }

            var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, MessageVerbosity.Info);

            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Homing Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                this.RequestingBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            if (this.stateData.StopRequestReason == StopRequestReason.NoReason)
            {
                using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                {
                    var setupStatusProvider = scope.ServiceProvider.GetRequiredService<ISetupStatusProvider>();

                    setupStatusProvider.CompleteVerticalOrigin();
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
