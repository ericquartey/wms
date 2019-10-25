using System;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.IODriver
{
    internal interface IIoStateMachine
    {
        #region Methods

        void ChangeState(IIoState newState);

        void EnqueueMessage(IoWriteMessage message);

        void ProcessMessage(IoMessage message);

        void ProcessResponseMessage(IoReadMessage message);

        void PublishNotificationEvent(FieldNotificationMessage notificationMessage);

        void Start();

        #endregion
    }
}
