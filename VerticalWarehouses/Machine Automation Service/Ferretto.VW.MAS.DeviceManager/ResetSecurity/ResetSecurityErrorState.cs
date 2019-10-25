using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.ResetSecurity.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetSecurity
{
    internal class ResetSecurityErrorState : StateBase
    {
        #region Fields

        private readonly IResetSecurityMachineData machineData;

        private readonly IResetSecurityStateData stateData;

        #endregion

        #region Constructors

        public ResetSecurityErrorState(IResetSecurityStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IResetSecurityMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ResetSecurity && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessage = new NotificationMessage(
                    null,
                    "Reset Security Stopped due to an error",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.ResetSecurity,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            var stopMessage = new FieldCommandMessage(
                null,
                $"Reset Security",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ResetSecurity,
                (byte)IoIndex.IoDevice1);

            this.Logger.LogTrace($"1:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            // do nothing
        }

        #endregion
    }
}
