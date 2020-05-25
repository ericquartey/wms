﻿using System;
using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ServicingInfo : DataModel
    {
        #region Properties

        public DateTime? InstallationDate { get; set; }

        public IEnumerable<Instruction> Instructions { get; set; }

        public bool IsHandOver => this.InstallationDate != null;

        public DateTime? LastServiceDate { get; set; }

        public MachineStatistics MachineStatistics { get; set; }

        public int? MachineStatisticsId { get; set; }

        public DateTime? NextServiceDate { get; set; }

        public MachineServiceStatus ServiceStatus { get; set; } = MachineServiceStatus.Valid;

        public int? TotalMissions { get; set; }

        #endregion
    }
}
