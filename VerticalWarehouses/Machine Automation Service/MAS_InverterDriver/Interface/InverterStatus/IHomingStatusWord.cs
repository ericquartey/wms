namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IHomingStatusWord : IStatusWord
    {
        #region Properties

        bool HomingAttained { get; }

        bool HomingError { get; }

        bool InternalLimitActive { get; }

        bool TargetReached { get; }

        #endregion
    }
}
