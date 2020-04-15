using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService
{
    public interface IInverterProvider
    {
        #region Properties

        IEnumerable<InverterDeviceInfo> GetStatuses { get; }

        #endregion

        #region Methods

        IEnumerable<InverterParameterSet> GetParameters();

        #endregion
    }
}
