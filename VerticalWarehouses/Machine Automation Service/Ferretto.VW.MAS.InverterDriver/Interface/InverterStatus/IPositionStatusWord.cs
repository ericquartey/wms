using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IPositionStatusWord : IStatusWord
    {
        #region Properties

        bool FollowingError { get; }

        bool PositioningAttained { get; }

        bool SetPointAcknowledge { get; }

        #endregion
    }
}
