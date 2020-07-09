using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.TelemetryService
{
    public interface ITelemetryWebHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task SendErrorLogAsync(string serialNumber, ErrorLog errorLog);

        Task SendMachineAsync(Machine machine);

        Task SendMissionLogAsync(string serialNumber, MissionLog missionLog);

        Task SendScreenCastAsync(int bayNumer, string serialNumber, byte[] screenshot);

        Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot);

        #endregion
    }
}
