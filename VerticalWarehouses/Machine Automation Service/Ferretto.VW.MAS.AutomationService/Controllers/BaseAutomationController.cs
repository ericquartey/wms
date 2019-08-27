using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public class BaseAutomationController : ControllerBase
    {

        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        protected BaseAutomationController(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
        }

        #endregion



        #region Properties

        public BayIndex BayIndex { get; set; }

        #endregion



        #region Methods

        protected void PublishCommand(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageType messageType)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        receiver,
                        MessageActor.WebApi,
                        messageType,
                        this.BayIndex));
        }

        protected void PublishNotification(
            IMessageData data,
            string description,
            MessageActor receiver,
            MessageType type,
            MessageStatus status,
            ErrorLevel level = ErrorLevel.NoError)
        {
            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage(
                        data,
                        description,
                        receiver,
                        MessageActor.WebApi,
                        type,
                        this.BayIndex,
                        status,
                        level));
        }

        protected TData WaitForResponseEventAsync<TData>(
            MessageType messageType,
            MessageActor messageSource = MessageActor.Any,
            MessageStatus? messageStatus = null,
            Action action = null)
            where TData : class, IMessageData
        {
            TData messageData = null;

            using (var semaphore = new Semaphore(0, 1))
            {
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

                const int MaximumWaitingTime = 10000;
                var signalReceived = semaphore.WaitOne(MaximumWaitingTime);

                notificationEvent.Unsubscribe(subscriptionToken);
                if (signalReceived == false)
                {
                    throw new TimeoutException("Waiting for the soecified event timed out.");
                }
            }

            return messageData;
        }

        #endregion
    }
}
