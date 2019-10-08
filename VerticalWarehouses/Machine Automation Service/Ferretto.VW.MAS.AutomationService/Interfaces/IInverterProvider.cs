using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Interfaces
{
    public interface IInverterProvider
    {
        #region Properties

        IEnumerable<InverterDeviceInfo> GetStatuses { get; }

        #endregion
    }
}
