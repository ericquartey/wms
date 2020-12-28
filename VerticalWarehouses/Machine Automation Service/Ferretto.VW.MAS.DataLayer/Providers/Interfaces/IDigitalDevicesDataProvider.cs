using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDigitalDevicesDataProvider
    {
        #region Methods

        void AddInverterParameter(InverterIndex inverterIndex, short code, int dataset, bool isReadOnly, string type, string value);

        bool CheckInverterParametersValidity(InverterIndex index);

        bool ExistInverterParameter(InverterIndex inverterIndex, short code);

        IEnumerable<Inverter> GetAllInverters();

        IEnumerable<Inverter> GetAllInvertersByBay(BayNumber bayNumber);

        IEnumerable<IoDevice> GetAllIoDevices();

        IEnumerable<Inverter> GetAllParameters();

        Inverter GetInverterByIndex(InverterIndex mainInverter);

        void UpdateInverterParameter(InverterIndex inverterIndex, short code, string value);

        #endregion
    }
}
