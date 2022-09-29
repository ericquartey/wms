using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class MachineControlProvider : BaseProvider, IMachineControlProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        #endregion

        #region Constructors

        public MachineControlProvider(
            IBaysDataProvider baysDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
        }

        #endregion

        #region Methods

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.PowerEnable ||
                notification.Type == MessageType.Stop ||
                notification.Type == MessageType.InverterStop ||
                notification.Type == MessageType.InverterFaultReset ||
                notification.Type == MessageType.InverterPowerEnable ||
                notification.Type == MessageType.ResetSecurity ||
                notification.Status == MessageStatus.OperationError ||
                notification.Status == MessageStatus.OperationStop ||
                notification.Status == MessageStatus.OperationFaultStop ||
                notification.Status == MessageStatus.OperationRunningStop);
        }

        public MessageStatus InverterPowerChangeStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.InverterPowerEnable)
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        public MessageStatus PowerStatusChangeStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.PowerEnable
                || message.Type == MessageType.InverterPowerEnable)
            {
                return message.Status;
            }

            return MessageStatus.NotSpecified;
        }

        public void ResetBayFault(BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bayNumber in this.baysDataProvider.GetBayNumbers())
                {
                    var description = $"Requesting inverter fault reset from bay {requestingBay} to bay {bayNumber}";
                    this.PublishCommand(
                        null,
                        description,
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.InverterFaultReset,
                        requestingBay,
                        bayNumber);
                }
            }
            else
            {
                this.PublishCommand(
                    null,
                    $"Requesting inverter fault reset from bay {requestingBay} to bay {targetBay}",
                    MessageActor.DeviceManager,
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

            return MessageStatus.NotSpecified;
        }

        public void ResetSecurity(MessageActor sender, BayNumber requestingBay)
        {
            this.PublishCommand(
                null,
                $"Bay {requestingBay} requested Security Reset",
                MessageActor.DeviceManager,
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

            return MessageStatus.NotSpecified;
        }

        public void StartChangePowerStatus(IChangeRunningStateMessageData messageData, MessageActor sender, BayNumber requestingBay)
        {
            this.PublishCommand(
                new PowerEnableMessageData(messageData.Enable),
                $"Setting Vertimag power status to {messageData.Enable}",
                MessageActor.DeviceManager,
                sender,
                MessageType.PowerEnable,
                requestingBay,
                BayNumber.None);
        }

        public void StartInverterPowerChange(IInverterPowerEnableMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bayNumber in this.baysDataProvider.GetBayNumbers())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting Inverter Change power status to {messageData.Enable} from bay {requestingBay} to bay {bayNumber}",
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.InverterPowerEnable,
                        requestingBay,
                        bayNumber);
                }
            }
            else
            {
                this.PublishCommand(
                    messageData,
                    $"Requesting Inverter Change power status to {messageData.Enable} from bay {requestingBay} to bay {targetBay}",
                    MessageActor.DeviceManager,
                    sender,
                    MessageType.InverterPowerEnable,
                    requestingBay,
                    targetBay);
            }
        }

        public void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bayNumber in this.baysDataProvider.GetBayNumbers())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting operation machine stop from bay {requestingBay} to bay {bayNumber} for reason {messageData.StopReason}",
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.Stop,
                        requestingBay,
                        bayNumber);
                }
            }
            else
            {
                this.PublishCommand(
                    messageData,
                    $"Requesting operation machine stop from bay {requestingBay} to bay {targetBay} for reason {messageData.StopReason}",
                    MessageActor.DeviceManager,
                    sender,
                    MessageType.Stop,
                    requestingBay,
                    targetBay);
            }

            // TODO check if notification to Automation Service is required for this operation
        }

        public MessageStatus StopOperationStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.Stop
                || message.Type == MessageType.InverterStop)
            {
                return message.Status;
            }

            if (message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationStop ||
                message.Status == MessageStatus.OperationError ||
                message.Status == MessageStatus.OperationRunningStop)
            {
                return MessageStatus.OperationEnd;
            }

            return MessageStatus.NotSpecified;
        }

        #endregion
    }
}
