using System;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineErrorsService
    {
        #region Events

        event EventHandler<MachineErrorEventArgs> ErrorStatusChanged;

        #endregion

        #region Properties

        MachineError ActiveError { get; set; }

        bool AutoNavigateOnError { get; set; }

        string ViewErrorActive { get; }

        bool IsErrorZero(int activeErrorCode);

        #endregion
    }
}
