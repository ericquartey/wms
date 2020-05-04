using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDigitalDevicesDataProvider
    {
        #region Methods

        IEnumerable<Inverter> GetAllInverters();

        IEnumerable<Inverter> GetAllInvertersByBay(BayNumber bayNumber);

        IEnumerable<IoDevice> GetAllIoDevices();

        IEnumerable<Inverter> GetAllParameters();

        Inverter GetInverterByIndex(InverterIndex mainInverter);

        #endregion
    }
}
