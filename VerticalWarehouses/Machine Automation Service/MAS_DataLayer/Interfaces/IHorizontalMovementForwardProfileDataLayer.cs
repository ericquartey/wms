using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementForwardProfileDataLayer
    {
        #region Properties

        Task<decimal> MovementCorrection { get; }

        Task<decimal> P0Acceleration { get; }

        Task<decimal> P0Deceleration { get; }

        Task<decimal> P0Quote { get; }

        Task<decimal> P0SpeedV1 { get; }

        Task<decimal> P1Acceleration { get; }

        Task<decimal> P1Deceleration { get; }

        Task<decimal> P1Quote { get; }

        Task<decimal> P1SpeedV2 { get; }

        Task<decimal> P2Acceleration { get; }

        Task<decimal> P2Deceleration { get; }

        Task<decimal> P2Quote { get; }

        Task<decimal> P2SpeedV3 { get; }

        Task<decimal> P3Acceleration { get; }

        Task<decimal> P3Deceleration { get; }

        Task<decimal> P3Quote { get; }

        Task<decimal> P3SpeedV4 { get; }

        Task<decimal> P4Acceleration { get; }

        Task<decimal> P4Deceleration { get; }

        Task<decimal> P4Quote { get; }

        Task<decimal> P4SpeedV5 { get; }

        Task<decimal> P5Acceleration { get; }

        Task<decimal> P5Deceleration { get; }

        Task<decimal> P5Quote { get; }

        Task<decimal> P5Speed { get; }

        Task<decimal> TotalMovement { get; }

        Task<int> TotalSteps { get; }

        #endregion
    }
}
