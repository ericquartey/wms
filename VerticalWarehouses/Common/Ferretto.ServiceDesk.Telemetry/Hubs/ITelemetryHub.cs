using System;
using System.Net;
using System.Threading.Tasks;

namespace Ferretto.ServiceDesk.Telemetry.Hubs
{
    public interface ITelemetryHub
    {
        #region Methods

        Task GetProxy(WebProxy webProxy);

        Task PersistIOLog(IOLog ioLog);

        Task RequestMachine();

        Task SendErrorLog(ErrorLog errorLog);

        Task SendIOLog(IOLog ioLog);

        Task SendMachine(Machine machine);

        Task SendMissionLog(MissionLog missionLog);

        Task SendProxy(Proxy proxy);

        Task SendRawDatabaseContent(byte[] rawDatabaseContent);

        Task SendScreenCast(int bayNumber, DateTimeOffset timeStamp, byte[] images);

        Task SendScreenShot(int bayNumber, DateTimeOffset timeStamp, string viewName, byte[] screenshot);

        Task SendServicingInfo(ServicingInfo servicingInfo);

        #endregion
    }
}
