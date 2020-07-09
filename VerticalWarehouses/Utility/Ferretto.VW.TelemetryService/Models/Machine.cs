using Ferretto.ServiceDesk.Telemetry.Models;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class Machine : RealmObject, IMachine
    {
        #region Properties

        public string ModelName { get; set; }

        public string SerialNumber { get; set; }

        #endregion
    }
}
