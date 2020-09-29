using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.TelemetryService
{
    public interface ITelemetryWebHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task PersistIOLogAsync(string serialNumber, IOLog ioLog);

        Task SendErrorLogAsync(string serialNumber, ErrorLog errorLog);

        Task SendIOLogAsync(string serialNumber, IOLog ioLog);

        Task SendMachineAsync(Machine machine);

        Task SendMissionLogAsync(string serialNumber, MissionLog missionLog);

        Task SendScreenCastAsync(int bayNumer, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot);

        Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot);

        #endregion
    }
}
