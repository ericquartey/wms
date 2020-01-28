using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineVolatileDataProvider
    {
        #region Properties

        MachineMode Mode { get; set; }

        Dictionary<BayNumber, bool> IsBayLightOn { get; }

        bool IsHomingExecuted { get; set; }

        bool IsMachineRunning { get; set; }

        #endregion
    }
}
