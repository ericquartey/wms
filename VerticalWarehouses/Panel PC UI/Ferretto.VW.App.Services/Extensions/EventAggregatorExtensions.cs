using System;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public static class EventAggregatorExtensions
    {
        #region Methods

        public static SubscriptionToken SubscribeToEvent<TMessageData>(
            this IEventAggregator eventAggregator,
            Action<NotificationMessageUI<TMessageData>> action,
            Predicate<NotificationMessageUI<TMessageData>> filter)
            where TMessageData : class, IMessageData
        {
            return eventAggregator
                .GetEvent<NotificationEventUI<TMessageData>>()
                .Subscribe(
                    action,
                    ThreadOption.UIThread,
                    false,
                    message => message != null && filter(message));
        }

        public static SubscriptionToken SubscribeToEvent<TMessageData>(
            this IEventAggregator eventAggregator,
            Action<NotificationMessageUI<TMessageData>> action)
            where TMessageData : class, IMessageData
        {
            return eventAggregator
                .GetEvent<NotificationEventUI<TMessageData>>()
                .Subscribe(
                    action,
                    ThreadOption.UIThread,
                    false,
                    message => message != null);
        }

        #endregion
    }
}
