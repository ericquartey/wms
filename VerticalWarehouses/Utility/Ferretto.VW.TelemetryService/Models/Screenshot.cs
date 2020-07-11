using System;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class Screenshot : RealmObject, IScreenShot
    {
        #region Properties

        public int BayNumber { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        public byte[]? Image { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? ViewName { get; set; }

        #endregion
    }
}
