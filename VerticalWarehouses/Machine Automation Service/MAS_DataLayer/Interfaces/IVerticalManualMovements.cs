using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVerticalManualMovements
    {
        #region Properties

        Task<decimal> FeedRateVM { get; }

        Task<decimal> InitialTargetPositionVM { get; }

        Task<decimal> RecoveryTargetPositionVM { get; }

        #endregion
    }
}
