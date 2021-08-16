using System;
using System.Threading.Tasks;

namespace Ferretto.ServiceDesk.Telemetry.Hubs
{
    public interface ITelemetryWebHub
    {
        #region Methods

        Task SendErrorLog(string serialNumber, ErrorLog errorLog);

        Task SendMachine(string serialNumber, Machine machine);

        Task SendMissionLog(string serialNumber, MissionLog missionLog);

        Task SendRawDatabaseContent(string serialNumber, byte[] rawDatabaseContent);

        Task SendScreenCast(int bayNumber, string serialNumber, byte[] images);

        Task SendScreenShot(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot);

        Task SendServicingInfo(string serialNumber, ServicingInfo servicingInfo);

        #endregion
    }
}
