using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Positioning.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Positioning
{
    internal class PositioningEndState : StateBase
    {
        #region Fields

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningEndState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
        }

        #endregion

        #region Destructors

        ~PositioningEndState()
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
                case FieldMessageType.InverterStop:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            var notificationMessage = new NotificationMessage(
                               this.machineData.MessageData,
                               this.machineData.MessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                               MessageActor.FiniteStateMachines,
                               MessageActor.FiniteStateMachines,
                               MessageType.Positioning,
                               this.machineData.RequestingBay,
                               this.machineData.TargetBay,
                               MessageStatus.OperationStop);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
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
            this.Logger?.LogTrace("1:Method Start");

            lock (this.machineData.MachineSensorStatus)
            {
                using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                {
                    var elevatorProvider = scope.ServiceProvider.GetRequiredService<IElevatorProvider>();

                    this.machineData.MessageData.CurrentPosition = (this.machineData.MessageData.AxisMovement == Axis.Vertical)
                        ? elevatorProvider.VerticalPosition
                        : elevatorProvider.HorizontalPosition;
                }
            }

            var inverterIndex = this.machineData.CurrentInverterIndex;
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    this.machineData.MessageData.NumberCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.FiniteStateMachines,
                    FieldMessageType.InverterStop,
                    (byte)inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    this.machineData.MessageData.NumberCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    MessageActor.FiniteStateMachines,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));
                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
            else
            {
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    this.machineData.MessageData.NumberCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                    MessageActor.FiniteStateMachines,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));
                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Positioning End");
            }

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.AxisPosition, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter axis position status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
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
