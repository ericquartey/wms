using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class Machine : RealmObject, IMachine
    {
        #region Properties

        [PrimaryKey]
        public int Id { get; set; }

        public string ModelName { get; set; } = string.Empty!;

        public string SerialNumber { get; set; } = string.Empty!;

        public string Version { get; set; } = string.Empty!;

        #endregion
    }
}
