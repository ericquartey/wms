using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IWeightControl
    {
        #region Properties

        Task<decimal> FeedRateWC { get; }

        Task<decimal> RequiredToleranceWC { get; }

        Task<decimal> TestRun { get; }

        #endregion
    }
}
