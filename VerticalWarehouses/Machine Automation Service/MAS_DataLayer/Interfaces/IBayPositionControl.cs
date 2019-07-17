using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IBayPositionControl
    {
        #region Properties

        Task<decimal> FeedRateBP { get; }

        Task<decimal> StepValueBP { get; }

        #endregion
    }
}
