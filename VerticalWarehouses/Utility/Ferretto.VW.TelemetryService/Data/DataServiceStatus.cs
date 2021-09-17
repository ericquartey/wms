using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.TelemetryService.Data
{
    internal class DataServiceStatus : IDataServiceStatus
    {
        #region Properties

        public bool IsReady { get; set; }

        #endregion
    }
}
