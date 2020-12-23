using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;

namespace Ferretto.VW.MAS.DeviceManager.InverterReading.Interfaces
{
    internal interface IInverterReadingMachineData : IMachineData
    {
        #region Properties

        IEnumerable<InverterParametersData> InverterParametersData { get; }

        #endregion
    }
}
