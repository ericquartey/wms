using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ICellControl
    {
        #region Properties

        Task<decimal> FeedRateCC { get; }

        Task<decimal> StepValueCC { get; }

        #endregion
    }
}
