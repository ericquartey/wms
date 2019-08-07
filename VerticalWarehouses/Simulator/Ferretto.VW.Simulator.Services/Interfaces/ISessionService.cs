using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Services.Interfaces
{
    public interface ISessionService
    {
        #region Methods

        /// <summary>
        /// Shuts down the current machine.
        /// </summary>
        /// <returns>Returns True if the shutdown process is successfully initiated, False otherwise.</returns>
        bool Shutdown();

        #endregion
    }
}
