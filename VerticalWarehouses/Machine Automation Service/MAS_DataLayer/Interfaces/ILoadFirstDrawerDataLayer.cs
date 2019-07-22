using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ILoadFirstDrawerDataLayer
    {
        #region Properties

        Task<decimal> FeedRateLFD { get; }

        #endregion
    }
}
