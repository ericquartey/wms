using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public class ErrorLog : IErrorLog
    {
        #region Properties

        public string AdditionalText { get; set; }

        public int BayNumber { get; set; }

        public int Code { get; set; }

        public int DetailCode { get; set; }

        public int ErrorId { get; set; }

        public int InverterIndex { get; set; }

        public DateTimeOffset OccurrenceDate { get; set; }

        public DateTimeOffset? ResolutionDate { get; set; }

        #endregion
    }
}
