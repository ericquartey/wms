using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    internal interface ISensorsStatesHubClient
    {
        #region Events

        event EventHandler<SensorsStatesEventArgs> SensorsStatesChanged;

        #endregion Events

        #region Methods

        Task ConnectAsync();

        #endregion Methods
    }
}
