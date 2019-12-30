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

        private readonly IErrorsProvider errorsProvider;

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
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
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
                            var notificationMessageData = new HomingMessageData(this.machineData.RequestedAxisToCalibrate, this.machineData.CalibrationType, this.machineData.LoadingUnitId, MessageVerbosity.Info);

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
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.RequestingBay);
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
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex}");
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
                var notificationMessageData = new HomingMessageData(this.machineData.RequestedAxisToCalibrate, this.machineData.CalibrationType, this.machineData.LoadingUnitId, MessageVerbosity.Info);

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

                if (this.machineData.AxisToCalibrate == Axis.Horizontal
                    ||
                    this.machineData.AxisToCalibrate == Axis.HorizontalAndVertical)
                {
                    this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>().UpdateLastIdealPosition(0);

                    if (!this.machineData.LoadingUnitId.HasValue
                        && this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle
                        )
                    {
                        this.scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>().UpdateHomingMissions(BayNumber.ElevatorBay, this.machineData.AxisToCalibrate);
                    }
                }
                else if (this.machineData.AxisToCalibrate == Axis.BayChain)
                {
                    this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>().UpdateLastIdealPosition(0, this.machineData.RequestingBay);

                    if (!this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.TargetBay)
                        && !this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.TargetBay)
                        )
                    {
                        this.scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>().UpdateHomingMissions(this.machineData.RequestingBay, this.machineData.AxisToCalibrate);
                    }
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
