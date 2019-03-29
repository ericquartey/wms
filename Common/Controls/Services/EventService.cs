using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Prism.Events;

namespace Ferretto.Common.Controls.Services
{
    public class EventService : IEventService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly Dictionary<string, List<PublicSubEventAction>> registeredEvents;

        private readonly Hashtable subscriptionTokens = new Hashtable();

        #endregion

        #region Constructors

        public EventService(IEventAggregator eventAggregator, INavigationService navigationService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.registeredEvents = new Dictionary<string, List<PublicSubEventAction>>();
        }

        #endregion

        #region Methods

        public void DynamicInvoke(IPubSubEvent eventArgs)
        {
            var entityKey = GetEntiykeyFromPubSubEvent(eventArgs);
            lock (this.registeredEvents)
            {
                if (this.registeredEvents.ContainsKey(entityKey))
                {
                    var pubSubscriptionsEvents = this.registeredEvents[entityKey];
                    foreach (var publishSubscription in pubSubscriptionsEvents)
                    {
                        publishSubscription.ActionPublish(eventArgs);
                    }
                }
            }
        }

        public void Invoke<TEventArgs>(TEventArgs eventArgs)
                    where TEventArgs : IPubSubEvent
        {
            this.GetEventBus<TEventArgs>()?.Publish(eventArgs);
        }

        public object Subscribe<TEventArgs>(
                Action<TEventArgs> action,
                string token,
                bool keepSubscriberReferenceAlive,
                bool forceUiThread = false)
            where TEventArgs : IPubSubEvent
        {
            var actualThread = this.navigationService.IsUnitTest || forceUiThread == false
                ? ThreadOption.PublisherThread
                : ThreadOption.UIThread;

            return this.GetEventBus<TEventArgs>().Subscribe(
                action,
                actualThread,
                keepSubscriberReferenceAlive,
                x => string.IsNullOrEmpty(x.Token) || x.Token == token);
        }

        public object Subscribe<TEventArgs>(Action<TEventArgs> action, bool forceUiThread = false)
            where TEventArgs : IPubSubEvent
        {
            return this.GetEventBus<TEventArgs>()
                .Subscribe(action, forceUiThread ? ThreadOption.UIThread : ThreadOption.PublisherThread, true);
        }

        public void Unsubscribe<TEventArgs>(object subscriptionToken)
           where TEventArgs : IPubSubEvent
        {
            var eventBus = this.GetEventBus<TEventArgs>();
            lock (this.registeredEvents)
            {
                if (this.subscriptionTokens.ContainsKey(subscriptionToken))
                {
                    var entityKey = this.subscriptionTokens[subscriptionToken].ToString();
                    if (this.registeredEvents.ContainsKey(entityKey))
                    {
                        var subscriptions = this.registeredEvents[entityKey];
                        var subscriptionToRemove = subscriptions.Single(s => s.Token == subscriptionToken);
                        subscriptions.Remove(subscriptionToRemove);
                        this.subscriptionTokens.Remove(subscriptionToken);
                    }
                }
            }

            eventBus.Unsubscribe(subscriptionToken as SubscriptionToken);
        }

        private static string GetEntiykeyFromPubSubEvent(IPubSubEvent eventArgs)
        {
            var st = Array.ConvertAll<Type, string>(eventArgs.GetType().GenericTypeArguments, Convert.ToString);
            var eventType = $"{eventArgs.GetType().Namespace}.{eventArgs.GetType().Name}";
            return string.Join(",", eventType, string.Join(",", st));
        }

        private PubSubEvent<TEventArgs> GetEventBus<TEventArgs>()
                    where TEventArgs : IPubSubEvent
        {
            return this.eventAggregator.GetEvent<PubSubEvent<TEventArgs>>();
        }

        #endregion
    }
}
