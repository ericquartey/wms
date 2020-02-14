using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;


namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class BaseProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        protected BaseProvider(IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        protected void PublishCommand(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageActor sender,
            MessageType messageType,
            BayNumber requestingBay,
            BayNumber targetBay)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        receiver,
                        sender,
                        messageType,
                        requestingBay,
                        targetBay));
        }

        protected void PublishNotification(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageActor sender,
            MessageType messageType,
            BayNumber requestingBay,
            BayNumber targetBay,
            MessageStatus status,
            ErrorLevel level)
        {
            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage(
                        messageData,
                        description,
                        receiver,
                        sender,
                        messageType,
                        requestingBay,
                        targetBay,
                        status,
                        level));
        }

        #endregion
    }
}
