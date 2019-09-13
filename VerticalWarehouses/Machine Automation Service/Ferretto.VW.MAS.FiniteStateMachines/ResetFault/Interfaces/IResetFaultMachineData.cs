using System.Collections.Generic;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.ResetFault.Interfaces
{
    public interface IResetFaultMachineData : IMachineData
    {


        #region Properties

        List<InverterIndex> BayInverters { get; }

        #endregion
    }
}
