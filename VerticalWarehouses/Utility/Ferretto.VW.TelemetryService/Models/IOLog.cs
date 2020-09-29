using System;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class IOLog : RealmObject, IIOLog
    {
        #region Properties

        public int BayNumber { get; set; }

        public string? Description { get; set; }

        public string? Input { get; set; }

        public Machine? Machine { get; set; }

        public string? Output { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        #endregion
    }
}
