using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ServicingInfo
    {
        #region Properties

        public int Id { get; set; }

        public DateTime? InstallationDate { get; set; }

        public DateTime? LastServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        public int? TotalMissions { get; set; }

        public MachineServiceStatus ServiceStatus { get; set; } = MachineServiceStatus.Valid;

        #endregion
    }
}
