using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IHorizontalAxis
    {
        #region Properties

        Task<decimal> AntiClockWiseRun { get; }

        Task<decimal> ClockWiseRun { get; }

        Task<bool> HomingExecutedHA { get; }

        Task<decimal> MaxEmptyAccelerationHA { get; }

        Task<decimal> MaxEmptyDecelerationHA { get; }

        Task<decimal> MaxEmptySpeedHA { get; }

        Task<decimal> MaxFullAcceleration { get; }

        Task<decimal> MaxFullDeceleration { get; }

        Task<decimal> MaxFullSpeed { get; }

        Task<decimal> OffsetHA { get; }

        Task<decimal> ResolutionHA { get; }

        #endregion
    }
}
