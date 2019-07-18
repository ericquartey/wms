using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementBackwardProfile
    {
        #region Properties

        Task<decimal> InitialSpeedBP { get; }

        Task<decimal> Step1AccDecBP { get; }

        Task<decimal> Step1PositionBP { get; }

        Task<decimal> Step1SpeedBP { get; }

        Task<decimal> Step2AccDecBP { get; }

        Task<decimal> Step2PositionBP { get; }

        Task<decimal> Step2SpeedBP { get; }

        Task<decimal> Step3AccDecBP { get; }

        Task<decimal> Step3PositionBP { get; }

        Task<decimal> Step3SpeedBP { get; }

        Task<decimal> Step4AccDecBP { get; }

        Task<decimal> Step4PositionBP { get; }

        Task<decimal> Step4SpeedBP { get; }

        Task<int> TotalSteps { get; }

        #endregion
    }
}
