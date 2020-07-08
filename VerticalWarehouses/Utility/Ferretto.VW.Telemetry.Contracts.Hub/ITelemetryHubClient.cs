using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public interface ITelemetryHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task SendErrorLog(string serialNumber, ErrorLog errorLog);

        Task SendMissionLog(string serialNumber, MissionLog missionLog);

        Task SendScreenCastAsync(int bayNumer, string serialNumber, byte[] screenshot);

        Task SendScreenShotAsync(int bayNumber, string machineSerial, DateTimeOffset timeSpan, byte[] screenShot);

        #endregion
    }
}
