using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IPositioningInverterStatus : IInverterStatusBase
    {
        #region Properties

        IPositionControlWord PositionControlWord { get; }

        IPositionStatusWord PositionStatusWord { get; }

        ITableTravelControlWord TableTravelControlWord { get; }

        ITableTravelStatusWord TableTravelStatusWord { get; }

        #endregion

        #region Methods

        bool UpdateInverterCurrentPosition(Axis axisToMove, int position);

        #endregion
    }
}
