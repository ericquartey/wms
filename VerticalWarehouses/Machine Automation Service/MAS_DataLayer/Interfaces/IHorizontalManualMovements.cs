using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalManualMovements
    {
        #region Properties

        Task<decimal> FeedRateHM { get; }

        Task<decimal> InitialTargetPositionHM { get; }

        Task<decimal> RecoveryTargetPositionHM { get; }

        #endregion
    }
}
