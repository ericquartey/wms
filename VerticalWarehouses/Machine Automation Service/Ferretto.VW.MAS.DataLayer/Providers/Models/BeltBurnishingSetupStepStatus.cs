using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Providers.Models
{
    public class BeltBurnishingSetupStepStatus : SetupStepStatus
    {
        #region Properties

        public new static BeltBurnishingSetupStepStatus Complete => new BeltBurnishingSetupStepStatus { CanBePerformed = true, IsCompleted = true };

        public int CompletedCycles { get; set; }

        #endregion
    }
}
