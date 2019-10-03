using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Interfaces
{
    internal interface IInverterPowerEnableMachineData : IMachineData
    {
        #region Properties

        IEnumerable<Inverter> BayInverters { get; }

        bool Enable { get; }

        #endregion
    }
}
