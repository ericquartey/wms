using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.Statistics.Interfaces
{
    internal interface IStatisticsMachineData : IMachineData
    {
        #region Properties

        InverterIndex InverterIndex { get; set; }

        #endregion
    }
}
