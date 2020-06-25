using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.Installer.Core
{
    public interface ISetupModeService
    {
        #region Properties

        SetupMode Mode { get; set; }

        #endregion
    }
}
