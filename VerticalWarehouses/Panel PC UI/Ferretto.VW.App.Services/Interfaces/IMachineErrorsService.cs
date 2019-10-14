using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IMachineErrorsService
    {
        #region Properties

        MAS.AutomationService.Contracts.Error ActiveError { get; set; }

        bool AutoNavigateOnError { get; set; }

        #endregion
    }
}
