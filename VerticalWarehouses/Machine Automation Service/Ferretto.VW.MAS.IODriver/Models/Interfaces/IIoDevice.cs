using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.IODriver
{
    internal interface IIoDevice
    {
        #region Methods

        void DestroyStateMachine();

        void ExecuteIoPowerUp();

        void ExecuteIoReset();

        void ExecuteMeasureProfile(FieldCommandMessage receivedMessage);

        void ExecutePowerEnable(FieldCommandMessage receivedMessage);

        void ExecuteResetSecurity();

        void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage);

        void ExecuteSetConfiguration();

        void ExecuteSwitchAxis(FieldCommandMessage receivedMessage);

        Task ReceiveIoDataTaskFunction();

        Task SendIoCommandTaskFunction();

        void SendIoMessageData(object state);

        void SendIoPublish(object state);

        Task StartHardwareCommunicationsAsync();

        void StartPollingIoMessage();

        #endregion
    }
}
