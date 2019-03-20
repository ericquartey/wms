using System;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public interface IInverterStateMachine : IDisposable
    {
        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">A new <see cref="IInverterState"/> state.</param>
        void ChangeState(IInverterState newState);

        /// <summary>
        /// Enqueue message.
        /// </summary>
        /// <param name="message">A <see cref="InverterMessage"/> message to enqueue.</param>
        void EnqueueMessage(InverterMessage message);

        /// <summary>
        /// Process a given message
        /// </summary>
        /// <param name="message">The <see cref="InverterMessage"/> message to be processed.</param>
        /// <returns></returns>
        bool ProcessMessage(InverterMessage message);

        /// <summary>
        /// On publishing a given notificcation message.
        /// </summary>
        /// <param name="notificationMessage">THe <see cref="NotificationMessage"/> message to be published.</param>
        void PublishNotificationEvent(NotificationMessage notificationMessage);

        /// <summary>
        /// Start states machine.
        /// </summary>
        void Start();

        #endregion
    }
}
