using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IInverterProvider
    {
        #region Properties

        IEnumerable<InverterDeviceInfo> GetStatuses { get; }

        #endregion

        #region Methods

        IEnumerable<Inverter> GetAllParameters();

        void SaveInverterStructure(IEnumerable<Inverter> inverters);

        #endregion
    }
}
