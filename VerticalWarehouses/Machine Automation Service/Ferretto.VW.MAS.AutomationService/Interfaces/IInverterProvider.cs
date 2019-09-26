using System.Collections.Generic;
using Ferretto.VW.MAS.AutomationService.Models;

namespace Ferretto.VW.MAS.AutomationService.Interfaces
{
    public interface IInverterProvider
    {
        #region Properties

        IEnumerable<InverterDevice> GetStatuses { get; }

        #endregion
    }
}
