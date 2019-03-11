using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class SensorsChangedEventArgs
    {
        #region Constructors

        public SensorsChangedEventArgs(bool[] states)
        {
            this.SensorsStates = states;
        }

        #endregion

        #region Properties

        public bool[] SensorsStates { get; set; }

        #endregion
    }
}
