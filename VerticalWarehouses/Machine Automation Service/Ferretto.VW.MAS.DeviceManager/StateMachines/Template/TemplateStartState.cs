using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Template.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.DeviceManager.Template
{
    internal class TemplateStartState : StateBase
    {
        #region Fields

        private readonly ITemplateMachineData machineData;

        private readonly ITemplateStateData stateData;

        #endregion

        #region Constructors

        public TemplateStartState(ITemplateStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ITemplateMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            if (message.Type == FieldMessageType.NoType)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new TemplateEndState(this.stateData));
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new TemplateErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var commandMessage = new FieldCommandMessage(
                null,
                $"Template Start State Field Command",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.NoType,
                (byte)InverterIndex.None);

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                $"Template Start State Notification with {this.machineData.Message}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.NotSpecified,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new TemplateEndState(this.stateData));
        }

        #endregion
    }
}
