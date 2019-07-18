using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVerticalAxis
    {
        #region Properties

        Task<decimal> DepositOffset { get; }

        Task<bool> HomingExecuted { get; }

        Task<decimal> HomingExitAcceleration { get; }

        Task<decimal> HomingExitDeceleration { get; }

        Task<decimal> HomingExitSpeed { get; }

        Task<decimal> HomingSearchAcceleration { get; }

        Task<decimal> HomingSearchDeceleration { get; }

        Task<bool> HomingSearchDirection { get; }

        Task<decimal> HomingSearchSpeed { get; }

        Task<decimal> LowerBound { get; }

        Task<decimal> MaxAcceleration { get; }

        Task<decimal> MaxDeceleration { get; }

        Task<decimal> MaxSpeed { get; }

        Task<decimal> Offset { get; }

        Task<decimal> Resolution { get; }

        Task<decimal> TakingOffset { get; }

        Task<decimal> UpperBound { get; }

        #endregion
    }
}
