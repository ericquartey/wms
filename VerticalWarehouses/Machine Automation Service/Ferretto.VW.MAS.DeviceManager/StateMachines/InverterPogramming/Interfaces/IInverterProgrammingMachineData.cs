using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;

namespace Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces
{
    internal interface IInverterProgrammingMachineData : IMachineData
    {
        #region Properties

        IEnumerable<InverterParametersData> InverterParametersData { get; }

        #endregion
    }
}
