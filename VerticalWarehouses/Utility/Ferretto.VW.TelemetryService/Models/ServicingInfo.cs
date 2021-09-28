using System;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class ServicingInfo : DataModel, IServicingInfo
    {
        #region Properties

        public DateTimeOffset? InstallationDate { get; set; }

        public bool IsHandOver { get; set; }

        public DateTimeOffset? LastServiceDate { get; set; }

        public DateTimeOffset? NextServiceDate { get; set; }

        public int ServiceStatusId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int? TotalMissions { get; set; }

        #endregion
    }
}
