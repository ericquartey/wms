using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces
{
    public interface IAngInverterStatus : IInverterStatusBase
    {
        #region Properties

        IHomingControlWord HomingControlWord { get; }

        IHomingStatusWord HomingStatusWord { get; }

        IPositionControlWord PositionControlWord { get; }

        IPositionStatusWord PositionStatusWord { get; }

        #endregion
    }
}
