using System;
using System.Threading.Tasks;

namespace Ferretto.ServiceDesk.Telemetry.Hubs
{
    public interface ITelemetryHub
    {
        #region Methods

        Task RequestMachine();

        Task SendErrorLog(ErrorLog errorLog);

        Task SendMachine(Machine machine);

        Task SendMissionLog(MissionLog missionLog);

        Task SendScreenCast(int bayNumber, DateTimeOffset timeStamp, byte[] images);

        Task SendScreenShot(int bayNumber, DateTimeOffset timeStamp, string viewName, byte[] screenshot);

        #endregion
    }
}
