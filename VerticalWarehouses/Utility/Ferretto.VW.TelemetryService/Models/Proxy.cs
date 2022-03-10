using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class Proxy : DataModel, IProxy
    {
        #region Properties

        public string? PasswordHash { get; set; } = string.Empty!;

        public string? PasswordSalt { get; set; } = string.Empty!;

        public string? Url { get; set; } = string.Empty!;

        public string? User { get; set; } = string.Empty!;

        #endregion
    }
}
