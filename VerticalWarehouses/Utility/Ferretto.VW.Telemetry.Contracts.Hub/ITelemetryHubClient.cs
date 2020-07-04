using System.Threading.Tasks;
using Ferretto.VW.Common.Hubs;
using Ferretto.VW.Telemetry.Contracts.Hub.Model;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public interface ITelemetryHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task SendLogsAsync(int bayNumer);

        Task SendMissionLog(string serialNumber, MissionLog missionLog);

        Task SendScreenshotAsync(int bayNumer, byte[] screenshot);

        #endregion
    }
}
