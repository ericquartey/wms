using System.Collections.Generic;
using Ferretto.VW.MachineAutomationService.Contracts;
using Ferretto.VW.MachineAutomationService.Hubs;

namespace Ferretto.WMS.Data.Core
{
    public interface IMachinesLiveDataContext
    {
        #region Properties

        IList<IMachineHubClient> MachineHubs { get; }

        #endregion

        #region Methods

        MachineStatus GetMachineStatus(int machineId);

        #endregion
    }
}
