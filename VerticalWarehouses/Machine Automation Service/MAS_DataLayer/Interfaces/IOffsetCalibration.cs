using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IOffsetCalibration
    {
        #region Properties

        Task<decimal> FeedRateOC { get; }

        Task<int> ReferenceCell { get; }

        Task<decimal> StepValue { get; }

        #endregion
    }
}
