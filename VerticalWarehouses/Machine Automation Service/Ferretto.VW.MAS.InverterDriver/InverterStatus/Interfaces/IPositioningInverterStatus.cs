using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IPositioningInverterStatus
    {
        #region Properties

        IPositionControlWord PositionControlWord { get; }

        #endregion

        #region Methods

        bool UpdateInverterCurrentPosition(Axis axisToMove, int position);

        #endregion
    }
}
