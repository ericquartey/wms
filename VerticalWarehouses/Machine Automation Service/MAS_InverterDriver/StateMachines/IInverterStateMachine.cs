using System;
using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public interface IInverterStateMachine : IDisposable
    {
        #region Methods

        void ChangeState(IInverterState newState);

        void EnqueueMessage(InverterMessage message);

        void ProcessMessage(InverterMessage message);

        void PublishNotificationEvent(NotificationMessage notificationMessage);

        void Start();

        #endregion
    }
}
