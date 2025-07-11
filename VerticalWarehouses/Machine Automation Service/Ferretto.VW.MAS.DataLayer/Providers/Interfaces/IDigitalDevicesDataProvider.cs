﻿using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IDigitalDevicesDataProvider
    {
        #region Methods

        void AddInverterParameter(InverterIndex inverterIndex, short code, int dataset, bool isReadOnly, string type, string value, string description, short writecode, short readcode, int decomalCount);

        bool ExistInverterParameter(InverterIndex inverterIndex, short code, int dataset);

        IEnumerable<Inverter> GetAllInverters();

        IEnumerable<Inverter> GetAllInvertersByBay(BayNumber bayNumber);

        IEnumerable<IoDevice> GetAllIoDevices();

        IEnumerable<Inverter> GetAllParameters();

        Inverter GetInverterByIndex(InverterIndex mainInverter);
        InverterType GetInverterTypeByIndex(InverterIndex index);
        InverterParameter GetParameter(InverterIndex inverterIndex, short code, int dataset);

        void SaveInverterStructure(Inverter inverter);

        void UpdateInverterParameter(InverterIndex inverterIndex, short code, string value, int dataset);

        void UpdateInverterParameters(List<InverterParameter> inverterParameters, byte index);

        #endregion
    }
}
