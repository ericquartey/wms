using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalAxis
    {
        #region Properties

        Task<decimal> AntiClockWiseRun { get; }

        Task<decimal> ClockWiseRun { get; }

        Task<bool> HomingExecutedHA { get; }

        Task<decimal> MaxAccelerationHA { get; }

        Task<decimal> MaxDecelerationHA { get; }

        Task<decimal> MaxSpeedHA { get; }

        Task<decimal> OffsetHA { get; }

        Task<decimal> ResolutionHA { get; }

        #endregion
    }
}
