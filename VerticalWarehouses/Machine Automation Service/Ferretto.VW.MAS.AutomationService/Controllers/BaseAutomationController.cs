using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
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
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        protected IActionResult NegativeResponse(Exception exception)
        {
            return this.NegativeResponse<ProblemDetails>(exception).Result;
        }

        protected ActionResult<T> NegativeResponse<T>(Exception exception)
        {
            if (exception is DataLayer.Exceptions.EntityNotFoundException)
            {
                return this.NotFound(new ProblemDetails
                {
                    Title = Resources.General.NotFoundTitle,
                    Detail = exception.Message
                });
            }
            else if (exception is ArgumentOutOfRangeException)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Title = Resources.General.BadRequestTitle,
                    Detail = exception.Message
                });
            }
            else if (exception is InvalidOperationException)
            {
                return this.UnprocessableEntity(new ProblemDetails
                {
                    Title = Resources.General.UnprocessableEntityTitle,
                    Detail = exception.Message
                });
            }
            else
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = Resources.General.InternalServerErrorTitle,
                    Detail = exception.Message
                });
            }
        }

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
                        this.BayNumber));
        }

        protected void PublishNotification(
            IMessageData data,
            string description,
            MessageActor receiver,
            MessageType type,
            MessageStatus status,
            BayNumber targetBay = BayNumber.None,
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
                        this.BayNumber,
                        targetBay,
                        status,
                        level));
        }

        #endregion

        /*
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
        */
    }
}
