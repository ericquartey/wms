using System;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class ErrorLog : RealmObject, IErrorLog
    {
        #region Properties

        public string? AdditionalText { get; set; }

        public int BayNumber { get; set; }

        public int Code { get; set; }

        public int DetailCode { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        public int InverterIndex { get; set; }

        public Machine? Machine { get; set; }

        public int MachineId { get; set; }

        public DateTimeOffset OccurrenceDate { get; set; }

        public DateTimeOffset? ResolutionDate { get; set; }

        #endregion
    }
}
