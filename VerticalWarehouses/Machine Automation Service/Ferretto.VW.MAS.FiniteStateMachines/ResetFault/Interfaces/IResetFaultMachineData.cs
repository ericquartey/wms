using System.Collections.Generic;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Interfaces
{
    internal interface IResetFaultMachineData : IMachineData
    {
        #region Properties

        List<InverterIndex> BayInverters { get; }

        #endregion
    }
}
