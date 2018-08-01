using System;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IEventService
  {
    void Invoke<T>(T eventArgs) where T : IEventArgs;
    /// <summary>
    /// Subscribes a delegate to an event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="action">The delegate that gets executed when the event is raised.</param>
    /// <param name="token">Specifies the subscriber token. Events will be routed only if the event's token matches the subscriber's token.</param>
    /// <param name="keepSubscriberReferenceAlive">When true, the EventService keeps a refernce to the subscriber so it does not get garbage collected.</param>
    void Subscribe<TEventArgs>(Action<TEventArgs> action, string token = "", bool keepSubscriberReferenceAlive = false) where TEventArgs : IEventArgs;
    void Unusbscribe<T>(Action<T> action) where T : IEventArgs;
  }
}
