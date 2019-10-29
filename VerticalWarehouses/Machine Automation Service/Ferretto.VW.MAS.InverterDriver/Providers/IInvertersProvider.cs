using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.InverterDriver
{
    public interface IInvertersProvider
    {
        #region Methods

        double ComputeDisplacement(double targetPosition);

        int ComputePositioningValues(
            IInverterStatusBase inverter,
            IPositioningFieldMessageData positioningData,
            Orientation axisOrientation,
            int currentPosition,
            bool refreshTargetTable,
            out InverterPositioningFieldMessageData positioningFieldData);

        int ConvertMillimetersToPulses(double millimeters, Orientation orientation);

        double ConvertPulsesToMillimeters(int pulses, Orientation orientation);

        IEnumerable<IInverterStatusBase> GetAll();

        IInverterStatusBase GetByIndex(InverterIndex index);

        IAngInverterStatus GetMainInverter();

        IInverterStatusBase GetShutterInverter(BayNumber bayNumber);

        #endregion
    }
}
