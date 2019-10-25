﻿using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class MachineControlProvider : BaseProvider, IMachineControlProvider
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

        public bool FilterNotifications(NotificationMessage notification, MessageActor destination)
        {
            return (notification.Destination == MessageActor.Any || notification.Destination == destination) &&
                (notification.Type == MessageType.PowerEnable ||
                notification.Type == MessageType.Stop ||
                notification.Type == MessageType.InverterFaultReset ||
                notification.Type == MessageType.InverterPowerEnable ||
                notification.Type == MessageType.ResetSecurity ||
                notification.Status == MessageStatus.OperationFaultStop ||
                notification.Status == MessageStatus.OperationRunningStop);
        }

        public MessageStatus InverterPowerChangeStatus(NotificationMessage message)
        {
            if (message.Type == MessageType.InverterPowerEnable)
            {
                return message.Status;
            }

            return MessageStatus.NoStatus;
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
                    var description = $"Requesting inverter fault reset from bay {requestingBay} to bay {bay.Number}";
                    this.PublishCommand(
                        null,
                        description,
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.InverterFaultReset,
                        requestingBay,
                        bay.Number);
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

            return MessageStatus.NoStatus;
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

            return MessageStatus.NoStatus;
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

            this.PublishNotification(
                messageData,
                $"Setting Vertimag power status to {messageData.Enable}",
                MessageActor.AutomationService,
                sender,
                MessageType.ChangeRunningState,
                requestingBay,
                BayNumber.None,
                MessageStatus.OperationStart,
                ErrorLevel.NoError
                );
        }

        public void StartInverterPowerChange(IInverterPowerEnableMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay)
        {
            if (targetBay == BayNumber.All)
            {
                foreach (var bay in this.baysProvider.GetAll())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting Inverter Change power status to {messageData.Enable} from bay {requestingBay} to bay {bay.Number}",
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.InverterPowerEnable,
                        requestingBay,
                        bay.Number);
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
                foreach (var bay in this.baysProvider.GetAll())
                {
                    this.PublishCommand(
                        messageData,
                        $"Requesting operation stop from bay {requestingBay} to bay {bay.Number} for reason {messageData.StopReason}",
                        MessageActor.DeviceManager,
                        sender,
                        MessageType.Stop,
                        requestingBay,
                        bay.Number);
                }
            }
            else
            {
                this.PublishCommand(
                    messageData,
                    $"Requesting operation stop from bay {requestingBay} to bay {targetBay} for reason {messageData.StopReason}",
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
