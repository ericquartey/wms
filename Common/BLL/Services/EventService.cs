using System;
using Ferretto.Common.BLL.Interfaces;
using Prism.Events;

namespace Ferretto.Common.BLL.Services
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

    public void Subscribe<TEventArgs>(Action<TEventArgs> action, string token, bool keepSubscriberReferenceAlive) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>().Subscribe(action, ThreadOption.BackgroundThread, keepSubscriberReferenceAlive, x => string.IsNullOrEmpty(x.Token) || x.Token == token);
    }

    public void Subscribe<TEventArgs>(Action<TEventArgs> action) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>().Subscribe(action, ThreadOption.BackgroundThread);
    }

    public void Unusbscribe<TEventArgs>(Action<TEventArgs> action) where TEventArgs : IEventArgs
    {
      GetEventBus<TEventArgs>().Unsubscribe(action);
    }

    private PubSubEvent<TEventArgs> GetEventBus<TEventArgs>() where TEventArgs : IEventArgs
    {
      return this.eventAggregator.GetEvent<PubSubEvent<TEventArgs>>();
    }
  }
}
