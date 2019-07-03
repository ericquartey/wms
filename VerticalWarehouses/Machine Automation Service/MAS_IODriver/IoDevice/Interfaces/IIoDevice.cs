using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_IODriver
{
    public interface IIoDevice
    {
        #region Methods

        Task ReceiveIoDataTaskFunction();

        Task SendIoCommandTaskFunction();

        Task StartHardwareCommunications();

        void StartPollingIoMessage();

        void SendIoMessageData(object state);

        void SendMessage(IFieldMessageData messageData);

        void ExecuteSwitchAxis(FieldCommandMessage receivedMessage);

        void ExecuteIoReset();

        void ExecuteIoPowerUp();

        void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage);

        void ExecuteSetConfiguration();

        void DestroyStateMachine();

        #endregion
    }
}
