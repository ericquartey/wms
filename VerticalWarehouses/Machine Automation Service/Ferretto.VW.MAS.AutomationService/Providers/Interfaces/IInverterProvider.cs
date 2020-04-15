using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IInverterProvider
    {
        #region Properties

        IEnumerable<InverterDeviceInfo> GetStatuses { get; }

        #endregion

        #region Methods

        IEnumerable<InverterParameterSet> GetAllParameters();

        InverterParameterSet GetParameters(InverterIndex index);

        #endregion
    }
}
