using System.Threading.Tasks;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface ITelemetryHubClient : IAutoReconnectHubClient
    {
        #region Methods

        Task SendLogsAsync(BayNumber bayNumer);

        Task SendScreenshotAsync(BayNumber bayNumer, byte[] image);

        #endregion
    }
}
