using System.Threading.Tasks;

namespace Ferretto.VW.TelemetryService
{
    public interface ITelemetryHub
    {
        #region Methods

        Task SendLogs(int bayNumber);

        Task SendMissionLog(string serialNumber, object missionLogs);

        Task SendScreenShot(int bayNumber, byte[] images);

        #endregion
    }
}
