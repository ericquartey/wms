using System;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public interface IIoStateMachine : IDisposable
    {
        #region Methods

        /// <summary>
        /// IO change State to new State
        /// </summary>
        /// <param name="newState"></param>
        void ChangeState(IIoState newState);

        /// <summary>
        /// IO command queue for producing message
        /// </summary>
        /// <param name="message"></param>
        void EnqueueMessage(IoMessage message);

        /// <summary>
        /// Process of message for change state
        /// </summary>
        /// <param name="message"></param>
        void ProcessMessage(IoMessage message);

        /// <summary>
        /// Publish a notify type message to another methods
        /// </summary>
        /// <param name="notificationMessage"></param>
        void PublishNotificationEvent(NotificationMessage notificationMessage);

        /// <summary>
        /// Start State Machines of IO
        /// </summary>
        void Start();

        #endregion
    }
}
