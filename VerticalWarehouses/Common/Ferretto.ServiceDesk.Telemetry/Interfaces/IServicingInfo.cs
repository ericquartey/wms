using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IServicingInfo
    {
        #region Properties

        int Id { get; set; }

        DateTimeOffset? InstallationDate { get; set; }

        bool IsHandOver { get; set; }

        DateTimeOffset? LastServiceDate { get; set; }

        DateTimeOffset? NextServiceDate { get; set; }

        int ServiceStatusId { get; set; }

        DateTimeOffset TimeStamp { get; set; }

        int? TotalMissions { get; set; }

        #endregion
    }
}
