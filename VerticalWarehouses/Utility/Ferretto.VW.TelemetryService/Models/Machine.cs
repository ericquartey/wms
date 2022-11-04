using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class Machine : DataModel, IMachine
    {
        #region Properties

        public string ModelName { get; set; } = string.Empty!;

        public byte[]? RawDatabaseContent { get; set; }

        public string SerialNumber { get; set; } = string.Empty!;

        public string Version { get; set; } = string.Empty!;

        public string WmsVersion { get; set; } = string.Empty!;

        #endregion
    }
}
