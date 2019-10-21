﻿using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineErrorsService
    {
        #region Properties

        MachineError ActiveError { get; set; }

        bool AutoNavigateOnError { get; set; }

        #endregion
    }
}
