using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IErrorLog
    {
        #region Properties

        string AdditionalText { get; set; }

        int BayNumber { get; set; }

        int Code { get; set; }

        int DetailCode { get; set; }

        int InverterIndex { get; set; }


        DateTimeOffset OccurrenceDate { get; set; }

        DateTimeOffset? ResolutionDate { get; set; }

        #endregion
    }
}
