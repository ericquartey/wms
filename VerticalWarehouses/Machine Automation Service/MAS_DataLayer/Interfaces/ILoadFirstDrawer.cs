using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface ILoadFirstDrawer
    {
        #region Properties

        Task<decimal> FeedRateLFD { get; }

        #endregion
    }
}
