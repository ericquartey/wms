using System;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class ScreenShot : RealmObject, IScreenShot
    {
        #region Properties

        public int BayNumber { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        public byte[]? Image { get; set; }

        public Machine? Machine { get; set; }

        public int MachineId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? ViewName { get; set; }

        #endregion
    }
}
