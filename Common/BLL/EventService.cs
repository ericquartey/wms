using System;
using Ferretto.Common.BLL.Interfaces;
using Prism.Events;

namespace Ferretto.Common.BLL
{
  public class EventService : IEventService
  {
    private readonly IEventAggregator eventAggregator;

    public EventService(IEventAggregator eventAggregator)
    {
      this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
    }

    public void Invoke<TEventArgs>(TEventArgs eventArgs) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>()?.Publish(eventArgs);
    }

    public void Subscribe<TEventArgs>(Action<TEventArgs> action, bool keepSubscriberReferenceAlive = false) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>()?.Subscribe(action, keepSubscriberReferenceAlive);
    }

    /// <summary>
    /// Subscribes a delegate to an event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="action">The delegate that gets executed when the event is raised.</param>
    /// <param name="token">Specifies the subscriber token. Events will be routed only if the event's token matches the subscriber's token.</param>
    /// <param name="keepSubscriberReferenceAlive">When true, the EventService keeps a refernce to the subscriber so it does not get garbage collected.</param>
    public void Subscribe<TEventArgs>(Action<TEventArgs> action, string token, bool keepSubscriberReferenceAlive = false) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>().Subscribe(action, ThreadOption.BackgroundThread, keepSubscriberReferenceAlive, x => string.IsNullOrEmpty(x.Token) || x.Token == token);
    }

    public void Unusbscribe<T>(Action<T> action) where T : IEventArgs
    {
      GetEventBus<T>().Unsubscribe(action);
    }

    private PubSubEvent<TEventArgs> GetEventBus<TEventArgs>() where TEventArgs : IEventArgs
    {
      return eventAggregator.GetEvent<PubSubEvent<TEventArgs>>();
    }
  }
}
