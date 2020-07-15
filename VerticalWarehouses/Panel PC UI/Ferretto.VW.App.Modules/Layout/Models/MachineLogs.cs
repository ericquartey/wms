using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.App.Modules.Layout.Models
{
    public class MachineLogs
    {
        #region Properties

        public IEnumerable<ErrorLog> ErrorLogs { get; set; }

        public IEnumerable<MissionLog> MissionLogs { get; set; }

        public IEnumerable<ScreenShot> ScreenShots { get; set; }

        #endregion
    }
}
