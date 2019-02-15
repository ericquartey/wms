using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class AutomationHubEventArgs
    {
        #region Constructors

        public AutomationHubEventArgs(string message)
        {
            this.Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion
    }
}
