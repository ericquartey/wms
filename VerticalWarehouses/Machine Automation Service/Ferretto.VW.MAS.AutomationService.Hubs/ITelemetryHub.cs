using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public interface ITelemetryHub
    {
        #region Methods

        Task SendLogs(BayNumber bayNumber);

        Task SendScreenShot(BayNumber bayNumber, byte[] images);

        #endregion
    }
}
