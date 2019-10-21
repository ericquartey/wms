using System;
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
    }
}
