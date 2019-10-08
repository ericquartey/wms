using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver
{
    public interface IInvertersProvider
    {
        #region Methods

        double ComputeDisplacement(double targetPosition);

        int ConvertMillimetersToPulses(double millimeters, Orientation orientation);

        double ConvertPulsesToMillimeters(int pulses, Orientation orientation);

        IEnumerable<IInverterStatusBase> GetAll();

        IInverterStatusBase GetByIndex(InverterIndex index);

        IAngInverterStatus GetMainInverter();

        #endregion
    }
}
