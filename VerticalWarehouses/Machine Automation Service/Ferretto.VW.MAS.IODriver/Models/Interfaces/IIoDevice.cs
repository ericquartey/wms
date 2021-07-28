using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS.IODriver
{
    internal interface IIoDevice
    {
        #region Properties

        bool IsCommandExecuting { get; }

        #endregion

        #region Methods

        void DestroyStateMachine();

        void ExecuteBayLight(FieldCommandMessage receivedMessage);

        void ExecuteEndMissionRobot(FieldCommandMessage receivedMessage);

        void ExecuteIoPowerUp();

        void ExecuteIoReset();

        void ExecuteMeasureProfile(FieldCommandMessage receivedMessage);

        void ExecutePowerEnable(FieldCommandMessage receivedMessage);

        void ExecuteReadyWarehouseRobot(FieldCommandMessage receivedMessage);

        void ExecuteResetSecurity();

        void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage);

        void ExecuteSetConfiguration();

        void ExecuteSwitchAxis(FieldCommandMessage receivedMessage);

        Task ReceiveIoDataTaskFunction(IHostingEnvironment env);

        Task SendIoCommandTaskFunction();

        void SendIoMessageData(object state);

        void SendIoPublish(object state);

        Task StartHardwareCommunicationsAsync();

        void StartPollingIoMessage();

        #endregion
    }
}
