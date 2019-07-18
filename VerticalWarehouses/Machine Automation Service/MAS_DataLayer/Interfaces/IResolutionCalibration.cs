using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IResolutionCalibration
    {
        #region Properties

        Task<decimal> FeedRate { get; }

        Task<decimal> FinalPosition { get; }

        Task<decimal> InitialPosition { get; }

        #endregion
    }
}
