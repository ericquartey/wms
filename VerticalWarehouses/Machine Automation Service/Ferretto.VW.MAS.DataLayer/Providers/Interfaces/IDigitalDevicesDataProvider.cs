using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDigitalDevicesDataProvider
    {
        #region Methods

        void AddInverterParameter(InverterIndex inverterIndex, short code, int dataset, bool isReadOnly, string type, string value, string description, short writecode, short readcode, int decomalCount);

        bool CheckInverterParametersValidity(InverterIndex index);

        bool ExistInverterParameter(InverterIndex inverterIndex, short code, int dataset);

        IEnumerable<Inverter> GetAllInverters();

        IEnumerable<Inverter> GetAllInvertersByBay(BayNumber bayNumber);

        IEnumerable<IoDevice> GetAllIoDevices();

        IEnumerable<Inverter> GetAllParameters();

        Inverter GetInverterByIndex(InverterIndex mainInverter);
        InverterParameter GetParameter(InverterIndex inverterIndex, short code, int dataset);
        void UpdateInverterParameter(InverterIndex inverterIndex, short code, string value, int dataset);

        #endregion
    }
}
