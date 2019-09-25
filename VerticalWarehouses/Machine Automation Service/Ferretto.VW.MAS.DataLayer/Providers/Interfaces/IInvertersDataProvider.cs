using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IDigitalDevicesDataProvider
    {
        #region Methods

        IEnumerable<Inverter> GetAllInverters();

        IEnumerable<IoDevice> GetAllIoDevices();

        Inverter GetInverterByIndex(InverterIndex mainInverter);

        #endregion
    }
}
