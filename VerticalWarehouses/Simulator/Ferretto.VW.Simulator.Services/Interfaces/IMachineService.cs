using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Services.Interfaces
{
    public interface IMachineService
    {
        #region Properties

        bool IsStartedSimulator { get; }

        bool NotIsStartedSimulator { get; }

        #endregion

        #region Methods

        Task ProcessStartSimulatorAsync();

        Task ProcessStopSimulatorAsync();

        #endregion
    }
}
