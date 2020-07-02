using System.Threading.Tasks;
using Ferretto.VW.Common.Hubs;

namespace Ferretto.VW.TelemetryService
{
    public interface ITelemetryWebHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task SendLogsAsync(int bayNumer);

        Task SendScreenshotAsync(int bayNumer, byte[] screenshot);

        #endregion
    }
}
