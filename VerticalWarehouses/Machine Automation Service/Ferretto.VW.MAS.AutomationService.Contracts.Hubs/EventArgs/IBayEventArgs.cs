using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public interface IBayEventArgs
    {
        #region Properties

        BayNumber BayNumber { get; }

        #endregion
    }
}
