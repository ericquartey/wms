using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.InverterDriver
{
    public interface IInvertersProvider
    {
        #region Methods

        double ComputeDisplacement(double targetPosition, out double weight);

        double ComputeDisplacement(double targetPosition, double weight);

        int ComputePositioningValues(
            IInverterStatusBase inverter,
            IPositioningFieldMessageData positioningData,
            Orientation axisOrientation,
            int currentPosition,
            bool refreshTargetTable,
            out InverterPositioningFieldMessageData positioningFieldData);

        int ConvertMillimetersToPulses(double millimeters, Orientation orientation);

        int ConvertMillimetersToPulses(double millimeters, IInverterStatusBase inverter);

        int ConvertMillimetersToPulses(double millimeters, InverterIndex bayInverterIndex);

        double ConvertPulsesToMillimeters(int pulses, Orientation orientation);

        double ConvertPulsesToMillimeters(int pulses, IInverterStatusBase inverter);

        IEnumerable<IInverterStatusBase> GetAll();

        IInverterStatusBase GetByIndex(InverterIndex index);

        #endregion
    }
}
