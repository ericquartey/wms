using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Source;

namespace BackgroundService
{
    public interface ISensorsStatesHub
    {
        #region Methods

        Task SensorsChanged(SensorsStates sensors);

        #endregion Methods
    }
}
