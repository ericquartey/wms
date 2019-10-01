using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class BaseProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        protected BaseProvider(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        protected void SendCommandToMissionManager(
            IMessageData messageData,
            string description,
            MessageActor sender,
            MessageType messageType,
            BayNumber requestingBay)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        MessageActor.MissionsManager,
                        sender,
                        messageType,
                        requestingBay));
        }

        protected TData WaitForResponseEventAsync<TData>(
            MessageType messageType,
            MessageActor messageSource = MessageActor.Any,
            MessageStatus? messageStatus = null,
            Action action = null,
            int timeoutInMilliseconds = 10000)
            where TData : class, IMessageData
        {
            TData messageData = null;

            var semaphore = new Semaphore(0, 100);

            var notificationEvent = this.eventAggregator
                .GetEvent<NotificationEvent>();

            var subscriptionToken = notificationEvent.Subscribe(
                m => { messageData = m.Data as TData; semaphore.Release(); },
                ThreadOption.PublisherThread,
                false,
                message =>
                    message.Type == messageType
                    &&
                    message.Data is TData
                    &&
                    (!messageStatus.HasValue || message.Status == messageStatus.Value)
                    &&
                    (messageSource == MessageActor.Any || message.Source == messageSource));

            action?.Invoke();

            var signalReceived = semaphore.WaitOne(timeoutInMilliseconds);

            notificationEvent.Unsubscribe(subscriptionToken);
            if (signalReceived == false)
            {
                throw new InvalidOperationException("Waiting for the specified event timed out.");
            }

            return messageData;
        }

        #endregion
    }
}
