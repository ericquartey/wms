using System;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class ErrorLog : DataModel, IErrorLog
    {
        #region Properties

        public string? AdditionalText { get; set; }

        public int BayNumber { get; set; }

        public int Code { get; set; }

        public int DetailCode { get; set; }

        public int ErrorId { get; set; }

        public int InverterIndex { get; set; }

        public Machine? Machine { get; set; }

        public int MachineId { get; set; }

        public DateTimeOffset OccurrenceDate { get; set; }

        public DateTimeOffset? ResolutionDate { get; set; }

        #endregion
    }
}
