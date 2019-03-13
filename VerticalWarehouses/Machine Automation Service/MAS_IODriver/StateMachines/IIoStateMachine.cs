using System;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public interface IIoStateMachine : IDisposable
    {
        #region Methods

        void ChangeState(IIoState newState);

        void EnqueueMessage(IoMessage message);

        void ProcessMessage(IoMessage message);

        void PublishNotificationEvent(NotificationMessage notificationMessage);

        /// <summary>
        ///
        /// </summary>
        void Start();

        #endregion
    }
}
