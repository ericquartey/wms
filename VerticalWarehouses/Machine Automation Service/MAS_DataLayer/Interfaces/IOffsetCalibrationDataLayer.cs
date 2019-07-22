using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IOffsetCalibrationDataLayer
    {
        #region Properties

        Task<decimal> FeedRateOC { get; }

        Task<int> ReferenceCell { get; }

        Task<decimal> StepValue { get; }

        #endregion
    }
}
