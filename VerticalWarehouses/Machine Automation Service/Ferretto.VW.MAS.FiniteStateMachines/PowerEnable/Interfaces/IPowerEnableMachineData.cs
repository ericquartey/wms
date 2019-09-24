using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces
{
    internal interface IPowerEnableMachineData : IMachineData
    {
        #region Properties

        List<BayNumber> ConfiguredBays { get; }

        bool Enable { get; }

        IMachineSensorsStatus MachineSensorStatus { get; }

        #endregion
    }
}
