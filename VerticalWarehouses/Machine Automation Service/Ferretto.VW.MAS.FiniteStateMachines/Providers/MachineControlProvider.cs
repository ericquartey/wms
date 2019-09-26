// ReSharper disable ArrangeThisQualifier

using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Providers
{
    public class MachineControlProvider : BaseProvider, IMachineControlProvider
    {

        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public MachineControlProvider(
            IBaysProvider baysProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
        }

        #endregion



        #region Methods

        public bool FilterCommands(CommandMessage command)
        {
            return command.Type == MessageType.ChangeRunningState;
        }

        public bool FilterNotifications(NotificationMessage notification)
        {
            return notification.Type == MessageType.PowerEnable ||
                notification.Type == MessageType.Stop ||
                notification.Type == MessageType.InverterFaultReset ||
                notification.Type == MessageType.ResetSecurity;
        }

        public MessageStatus PowerStatusChangeStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.PowerEnable)
            {
                return message.Status;
            }

            return MessageStatus.NoStatus;
        }

        public void ResetBayFault(BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bay in this.baysProvider.GetAll())
                {
                    this.PublishCommand(
                        null,
                        $"Requesting inverter fault reset from bay {requestingBay} to bay {bay.Index}",
                        MessageActor.FiniteStateMachines,
                        sender,
                        MessageType.InverterFaultReset,
                        requestingBay,
                        bay.Index);
                }
            }
            else
            {
                this.PublishCommand(
                    null,
                    $"Requesting inverter fault reset from bay {requestingBay} to bay {targetBay}",
                    MessageActor.FiniteStateMachines,
                    sender,
                    MessageType.InverterFaultReset,
                    requestingBay,
                    targetBay);
            }

            //TODO check if notification to Automation Service is required for this operation
        }

        public MessageStatus ResetBayFaultStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.InverterFaultReset)
            {
                return message.Status;
            }

            return MessageStatus.NoStatus;
        }

        public void ResetSecurity(MessageActor sender, BayNumber requestingBay)
        {
            this.PublishCommand(
                null,
                $"Bay {requestingBay} requested Security Reset",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.ResetSecurity,
                requestingBay,
                BayNumber.None);

            // TODO check if notification to Automation Service is required for this operation
        }

        public MessageStatus ResetSecurityStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.ResetSecurity)
            {
                return message.Status;
            }

            return MessageStatus.NoStatus;
        }

        public void StartChangePowerStatus(IChangeRunningStateMessageData messageData, MessageActor sender, BayNumber requestingBay)
        {
            this.PublishCommand(
                new PowerEnableMessageData(messageData.Enable),
                $"Setting Vertimag power status to {messageData.Enable}",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.PowerEnable,
                requestingBay,
                BayNumber.None);

            this.PublishNotification(
                messageData,
                $"Setting Vertimag power status to {messageData.Enable}",
                MessageActor.AutomationService,
                MessageActor.MissionsManager,
                MessageType.ChangeRunningState,
                requestingBay,
                BayNumber.None,
                MessageStatus.OperationStart,
                ErrorLevel.NoError
                );
        }

        public void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bay in this.baysProvider.GetAll())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting operation stop from bay {requestingBay} to bay {bay.Index} for reason {messageData.StopReason}",
                        MessageActor.FiniteStateMachines,
                        sender,
                        MessageType.Stop,
                        requestingBay,
                        bay.Index);
                }
            }
            else
            {
                this.PublishCommand(
                    messageData,
                    $"Requesting operation stop from bay {requestingBay} to bay {targetBay} for reason {messageData.StopReason}",
                    MessageActor.FiniteStateMachines,
                    sender,
                    MessageType.Stop,
                    requestingBay,
                    targetBay);
            }

            // TODO check if notification to Automation Service is required for this operation
        }

        public MessageStatus StopOperationStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Stop)
            {
                return message.Status;
            }

            if (message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationRunningStop)
            {
                return MessageStatus.OperationEnd;
            }

            return MessageStatus.NoStatus;
        }

        #endregion
    }
}
