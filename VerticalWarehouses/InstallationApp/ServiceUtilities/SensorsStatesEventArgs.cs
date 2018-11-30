using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class SensorsStatesEventArgs
    {
        #region Constructors

        public SensorsStatesEventArgs(SensorsStates sensors)
        {
            this.SensorsStates = sensors;
        }

        #endregion Constructors

        #region Properties

        public SensorsStates SensorsStates { get; private set; }

        #endregion Properties
    }
}
