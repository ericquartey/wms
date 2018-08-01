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

    public void Subscribe<TEventArgs>(Action<TEventArgs> action, string token = "", bool keepSubscriberReferenceAlive = false) where TEventArgs : IEventArgs
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
