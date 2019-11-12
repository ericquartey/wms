using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.InverterPowerEnable.Interfaces
{
    internal interface IInverterPowerEnableMachineData : IMachineData
    {
        #region Properties

        IEnumerable<Inverter> BayInverters { get; }

        bool Enable { get; }

        #endregion
    }
}
