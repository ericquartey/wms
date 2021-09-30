using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.TelemetryService.Data
{
    public interface IDataServiceStatus
    {
        #region Properties

        bool IsReady { get; set; }

        #endregion
    }
}
