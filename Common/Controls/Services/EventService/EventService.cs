using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Prism.Events;

namespace Ferretto.Common.Controls.Services
{
    public class EventService : IEventService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<string, List<PublicSubEventAction>> registeredEvents;

        private readonly Hashtable subscriptionTokens = new Hashtable();

        #endregion

        #region Constructors

        public EventService(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.registeredEvents = new Dictionary<string, List<PublicSubEventAction>>();
        }

        #endregion

        #region Methods

        public void DynamicInvoke(IPubSubEvent eventArgs)
        {
            var entityKey = this.GetEntiykeyFromPubSubEvent(eventArgs);

            if (this.registeredEvents.ContainsKey(entityKey))
            {
                var pubSubscriptionsEvents = this.registeredEvents[entityKey];
                foreach (var publishSubscription in pubSubscriptionsEvents)
                {
                    publishSubscription.ActionPublish(eventArgs);
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
            if (ServiceLocator.Current.GetInstance<INavigationService>().IsUnitTest)
            {
                forceUiThread = false;
            }

            var eventBus = this.GetEventBus<TEventArgs>();
            var subscriptionToken = eventBus.Subscribe(
                  action,
                  forceUiThread ? ThreadOption.UIThread : ThreadOption.PublisherThread,
                  keepSubscriberReferenceAlive,
                  x => string.IsNullOrEmpty(x.Token) || x.Token == token);

            this.Register(action, eventBus, subscriptionToken);
            return subscriptionToken;
        }

        public object Subscribe<TEventArgs>(Action<TEventArgs> action, bool forceUiThread = false)
               where TEventArgs : IPubSubEvent
        {
            var eventBus = this.GetEventBus<TEventArgs>();
            var subscriptionToken = eventBus.Subscribe(
                action,
                forceUiThread ? ThreadOption.UIThread : ThreadOption.PublisherThread,
                true);

            this.Register(action, eventBus, subscriptionToken);

            return subscriptionToken;
        }

        public void Unsubscribe<TEventArgs>(object subscriptionToken)
               where TEventArgs : IPubSubEvent
        {
            var eventBus = this.GetEventBus<TEventArgs>();
            lock (this.registeredEvents)
            {
                var entityKey = this.subscriptionTokens[subscriptionToken].ToString();
                if (this.registeredEvents.ContainsKey(entityKey))
                {
                    var subscriptions = this.registeredEvents[entityKey];
                    var subscriptionToRemove = subscriptions.Where(s => s.Token == subscriptionToken).Single();
                    subscriptions.Remove(subscriptionToRemove);
                    this.subscriptionTokens.Remove(subscriptionToken);
                }
            }

            eventBus.Unsubscribe(subscriptionToken as SubscriptionToken);
        }

        private void EventPublish<TEventArgs>(PubSubEvent<TEventArgs> eventBus, TEventArgs eventArgs)
                where TEventArgs : IPubSubEvent
        {
            eventBus.Publish(eventArgs);
        }

        private string GetActionEventKey<TEventArgs>(Action<TEventArgs> action)
                where TEventArgs : IPubSubEvent
        {
            if (action.GetType().GenericTypeArguments.Length == 0)
            {
                throw new ArgumentException(string.Format(Errors.EventServiceInvalidArguments, nameof(EventService)));
            }

            var st = Array.ConvertAll<Type, string>(action.Method.DeclaringType.GenericTypeArguments, Convert.ToString);
            var eventFullName = $"{action.GetType().GenericTypeArguments[0].Namespace}.{action.GetType().GenericTypeArguments[0].Name}";
            return string.Join(",", eventFullName, string.Join(",", st));
        }

        private string GetEntiykeyFromPubSubEvent(IPubSubEvent eventArgs)
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

        private void Register<TEventArgs>(Action<TEventArgs> action, PubSubEvent<TEventArgs> eventBus, SubscriptionToken token)
                        where TEventArgs : IPubSubEvent
        {
            var entityKey = this.GetActionEventKey(action);

            Action<object> eventAction = (e) => this.EventPublish(eventBus, (TEventArgs)e);

            var pubEventAction = new PublicSubEventAction(eventAction, token);
            lock (this.registeredEvents)
            {
                if (this.registeredEvents.ContainsKey(entityKey))
                {
                    this.registeredEvents[entityKey].Add(pubEventAction);
                }
                else
                {
                    this.registeredEvents[entityKey] = new List<PublicSubEventAction> { pubEventAction };
                }

                this.subscriptionTokens.Add(token, entityKey);
            }
        }

        #endregion
    }
}
