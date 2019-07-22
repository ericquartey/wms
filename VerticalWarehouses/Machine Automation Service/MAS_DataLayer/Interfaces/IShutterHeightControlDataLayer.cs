using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterHeightControlDataLayer
    {
        #region Properties

        Task<decimal> FeedRateSH { get; }

        Task<decimal> RequiredTolerance { get; }

        #endregion
    }
}
