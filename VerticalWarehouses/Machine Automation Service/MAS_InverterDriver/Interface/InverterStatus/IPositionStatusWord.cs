namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IPositionStatusWord : IStatusWord
    {
        #region Properties

        bool FollowingError { get; }

        bool SetPointAcknowledge { get; }

        bool PositioningAttained { get; }

        #endregion
    }
}
