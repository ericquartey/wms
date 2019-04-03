using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver.Interface.StateMachines
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
        /// Process a given message.
        /// </summary>
        /// <param name="message">The <see cref="InverterMessage"/> message to be processed.</param>
        /// <returns></returns>
        bool ProcessMessage(InverterMessage message);

        /// <summary>
        /// On publishing a given notification message.
        /// </summary>
        /// <param name="notificationMessage">THe <see cref="NotificationMessage"/> message to be published.</param>
        void PublishNotificationEvent(FieldNotificationMessage notificationMessage);

        /// <summary>
        /// Start states machine.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop states machine.
        /// </summary>
        void Stop();

        #endregion
    }
}
