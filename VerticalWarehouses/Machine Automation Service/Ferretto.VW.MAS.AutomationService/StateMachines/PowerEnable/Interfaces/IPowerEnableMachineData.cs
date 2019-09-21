using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable.Interfaces
{
    public interface IPowerEnableMachineData : IMachineData
    {


        #region Properties

        List<Bay> ConfiguredBays { get; }

        bool RequestedPowerState { get; }

        #endregion
    }
}
