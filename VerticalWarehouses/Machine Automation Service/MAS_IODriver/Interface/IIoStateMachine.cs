using System;
using Ferretto.VW.MAS_Utils.Messages;

namespace Ferretto.VW.MAS_IODriver.Interface
{
    public interface IIoStateMachine : IDisposable
    {
        #region Methods

        void ChangeState(IIoState newState);

        void EnqueueMessage(IoSHDWriteMessage message);

        void ProcessMessage(IoSHDMessage message);

        void ProcessResponseMessage(IoSHDReadMessage message);

        void PublishNotificationEvent(FieldNotificationMessage notificationMessage);

        void Start();

        #endregion
    }
}
