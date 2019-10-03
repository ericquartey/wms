using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Interfaces
{
    internal interface IResetFaultMachineData : Interface.IMachineData
    {
        #region Properties

        IEnumerable<Inverter> BayInverters { get; }

        #endregion
    }
}
