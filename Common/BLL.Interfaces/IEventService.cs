﻿using System;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IEventService
    {
        #region Methods

        void DynamicInvoke(IPubSubEvent eventArgs);

        void Invoke<T>(T eventArgs)
            where T : IPubSubEvent;

        /// <summary>
        /// Subscribes a delegate to an event.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="action">The delegate that gets executed when the event is raised.</param>
        /// <param name="token">Specifies the subscriber token. Events will be routed only if the event's token matches the subscriber's token.</param>
        /// <param name="keepSubscriberReferenceAlive">When true, the EventService keeps a reference to the subscriber so it does not get garbage collected.</param>
        /// <param name="forceUiThread">Force the call to be done on the UI thread, instead of the same thread on which the event was published.</param>
        object Subscribe<TEventArgs>(
                Action<TEventArgs> action,
                string token,
                bool keepSubscriberReferenceAlive,
                bool forceUiThread = false,
                Predicate<TEventArgs> filter = null)
            where TEventArgs : class, IPubSubEvent;

        /// <summary>
        /// Subscribes a delegate to an event.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="action">The delegate that gets executed when the event is raised.</param>
        /// <param name="forceUiThread">Force the call to be done on the UI thread, instead of the same thread on which the event was published.</param>
        object Subscribe<TEventArgs>(
            Action<TEventArgs> action,
            bool forceUiThread = false,
            Predicate<TEventArgs> filter = null)
            where TEventArgs : class, IPubSubEvent;

        void Unsubscribe<T>(object subscriptionToken)
            where T : class, IPubSubEvent;

        #endregion
    }
}
