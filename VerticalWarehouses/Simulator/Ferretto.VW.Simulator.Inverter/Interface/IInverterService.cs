using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Inverter.Interface
{
    internal interface IInverterService
    {
        #region Properties

        bool IsStartedSimulator { get; }

        #endregion

        #region Methods

        Task ProcessStartSimulatorAsync();

        #endregion
    }
}
