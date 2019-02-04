using System;
using Ferretto.Common.BLL.Interfaces;
using Prism.Events;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class EventService : IEventService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public EventService(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

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
            return this.GetEventBus<TEventArgs>().Subscribe(
                action,
                forceUiThread ? ThreadOption.UIThread : ThreadOption.PublisherThread,
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
            this.GetEventBus<TEventArgs>().Unsubscribe(subscriptionToken as SubscriptionToken);
        }

        private PubSubEvent<TEventArgs> GetEventBus<TEventArgs>()
            where TEventArgs : IPubSubEvent
        {
            return this.eventAggregator.GetEvent<PubSubEvent<TEventArgs>>();
        }

        #endregion
    }
}
