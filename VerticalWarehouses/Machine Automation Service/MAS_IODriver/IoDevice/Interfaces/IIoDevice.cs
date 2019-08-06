using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.IODriver.IoDevice.Interfaces
{
    public interface IIoDevice
    {
        #region Methods

        Task ReceiveIoDataTaskFunction();

        Task SendIoCommandTaskFunction();

        Task StartHardwareCommunications();

        void StartPollingIoMessage();

        void SendIoPublish(object state);

        void SendIoMessageData(object state);

        void SendMessage(IFieldMessageData messageData);

        void ExecuteSwitchAxis(FieldCommandMessage receivedMessage);

        void ExecuteIoReset();

        void ExecuteIoPowerUp();

        void ExecutePowerEnable(FieldCommandMessage receivedMessage);

        void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage);

        void ExecuteSetConfiguration();

        void DestroyStateMachine();

        #endregion
    }
}
