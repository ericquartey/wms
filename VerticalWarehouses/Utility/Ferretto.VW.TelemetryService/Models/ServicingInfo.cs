using System;
using Ferretto.ServiceDesk.Telemetry;
using Realms;

namespace Ferretto.VW.TelemetryService.Models
{
    public class ServicingInfo : RealmObject, IServicingInfo
    {
        #region Properties

        [PrimaryKey]
        public int Id { get; set; }

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
