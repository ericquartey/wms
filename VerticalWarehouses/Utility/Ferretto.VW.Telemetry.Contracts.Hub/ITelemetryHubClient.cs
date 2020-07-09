using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.Telemetry.Contracts.Hub
{
    public interface ITelemetryHubClient : IAutoReconnectHubClient
    {
        #region Events

        event EventHandler MachineReceivedChanged;

        #endregion

        #region Methods

        Task SendErrorLogAsync(ErrorLog errorLog);

        Task SendMachineAsync(Machine machine);

        Task SendMissionLogAsync(MissionLog missionLog);

        Task SendScreenCastAsync(int bayNumer, byte[] screenshot);

        Task SendScreenShotAsync(int bayNumber, DateTimeOffset timeSpan, byte[] screenShot);

        #endregion
    }
}
