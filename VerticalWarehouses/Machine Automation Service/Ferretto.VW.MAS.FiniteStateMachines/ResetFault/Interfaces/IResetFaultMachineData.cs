using System.Collections.Generic;

namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Interfaces
{
    internal interface IResetFaultMachineData : Interface.IMachineData
    {
        #region Properties

        IEnumerable<DataModels.Inverter> BayInverters { get; }

        #endregion
    }
}
