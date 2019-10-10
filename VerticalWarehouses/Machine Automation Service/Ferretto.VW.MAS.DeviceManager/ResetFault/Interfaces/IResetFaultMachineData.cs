using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces
{
    internal interface IResetFaultMachineData : IMachineData
    {
        #region Properties

        IEnumerable<Inverter> BayInverters { get; }

        #endregion
    }
}
