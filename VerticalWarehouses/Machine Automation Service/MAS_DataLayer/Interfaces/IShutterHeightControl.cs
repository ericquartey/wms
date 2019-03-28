using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IShutterHeightControl
    {
        #region Properties

        Task<decimal> FeedRateSH { get; }

        Task<decimal> RequiredTolerance { get; }

        #endregion
    }
}
