using System;
using Ferretto.VW.MAS_Utils.Messages;

namespace Ferretto.VW.MAS_IODriver.Interface
{
    public interface IIoStateMachine : IDisposable
    {
        #region Methods

        void ChangeState(IIoState newState);

        //void EnqueueMessage(IoMessage message);
        void EnqueueMessage(IoSHDMessage message);

        //void ProcessMessage(IoMessage message);
        void ProcessMessage(IoSHDMessage message);

        void PublishNotificationEvent(FieldNotificationMessage notificationMessage);

        void Start();

        #endregion
    }
}
