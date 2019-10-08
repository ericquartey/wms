using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface ITableTravelStatusWord : IStatusWord
    {
        #region Properties

        bool FollowingError { get; }

        bool InGear { get; }

        bool InternalLimitActive { get; }

        bool MotionBlockInProgress { get; }

        bool TargetReached { get; }

        #endregion
    }
}
