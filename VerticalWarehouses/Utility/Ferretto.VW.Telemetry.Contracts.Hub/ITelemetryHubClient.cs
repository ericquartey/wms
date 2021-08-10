using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public interface ITelemetryHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler MachineReceivedChanged;

        #endregion

        #region Methods

        Task SendErrorLogAsync(IErrorLog errorLog);

        Task SendIOLogAsync(IIOLog ioLog);

        Task SendMachineAsync(IMachine machine);

        Task SendMissionLogAsync(IMissionLog missionLog);

        Task SendRawDatabaseContentAsync(byte[] rawDatabaseContent);

        Task SendScreenCastAsync(int bayNumer, byte[] screenshot, DateTimeOffset dateTime);

        Task SendScreenShotAsync(int bayNumber, DateTimeOffset dateTime, byte[] screenShot);

        Task SendServicingInfoAsync(IServicingInfo servicingInfo);

        #endregion
    }
}
