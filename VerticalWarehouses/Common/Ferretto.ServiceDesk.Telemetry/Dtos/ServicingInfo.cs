using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public class ServicingInfo : IServicingInfo
    {
        #region Properties

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
