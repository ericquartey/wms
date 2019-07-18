using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface ICellControlDataLayer
    {
        #region Properties

        Task<decimal> FeedRateCC { get; }

        Task<decimal> StepValueCC { get; }

        #endregion
    }
}
