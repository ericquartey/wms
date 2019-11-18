using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Homing
{
    internal class HomingEndState : StateBase, System.IDisposable
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

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterPowerOff:
                case FieldMessageType.CalibrateAxis:
                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterSwitchOff:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:
                            var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, this.machineData.CalibrationType, MessageVerbosity.Info);

                            var notificationMessage = new NotificationMessage(
                                notificationMessageData,
                                "Homing Completed",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Homing,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
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
            // do nothing
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var targetInverter = this.machineData.CurrentInverterIndex;
                var stopMessage = new FieldCommandMessage(
                    null,
                    "Homing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterStop,
                    (byte)targetInverter);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                var notificationMessageData = new HomingMessageData(this.machineData.AxisToCalibrate, this.machineData.CalibrationType, MessageVerbosity.Info);

                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Completed",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Homing,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                if (this.machineData.AxisToCalibrate == Axis.Horizontal || this.machineData.AxisToCalibrate == Axis.HorizontalAndVertical)
                {
                    this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>().UpdatePositioningCompensation(Orientation.Horizontal, 0);
                }
            }

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
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        protected void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        #endregion
    }
}
