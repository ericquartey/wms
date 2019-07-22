using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IPanelControlDataLayer
    {
        #region Properties

        Task<int> BackInitialReferenceCell { get; }

        Task<int> BackPanelQuantity { get; }

        Task<decimal> FeedRatePC { get; }

        Task<int> FrontInitialReferenceCell { get; }

        Task<int> FrontPanelQuantity { get; }

        Task<decimal> StepValuePC { get; }

        #endregion
    }
}
