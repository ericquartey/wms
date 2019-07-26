namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementForwardProfileDataLayer
    {
        #region Properties

        decimal MovementCorrection { get; }

        decimal P0Acceleration { get; }

        decimal P0Deceleration { get; }

        decimal P0Quote { get; }

        decimal P0SpeedV1 { get; }

        decimal P1Acceleration { get; }

        decimal P1Deceleration { get; }

        decimal P1Quote { get; }

        decimal P1SpeedV2 { get; }

        decimal P2Acceleration { get; }

        decimal P2Deceleration { get; }

        decimal P2Quote { get; }

        decimal P2SpeedV3 { get; }

        decimal P3Acceleration { get; }

        decimal P3Deceleration { get; }

        decimal P3Quote { get; }

        decimal P3SpeedV4 { get; }

        decimal P4Acceleration { get; }

        decimal P4Deceleration { get; }

        decimal P4Quote { get; }

        decimal P4SpeedV5 { get; }

        decimal P5Acceleration { get; }

        decimal P5Deceleration { get; }

        decimal P5Quote { get; }

        decimal P5Speed { get; }

        decimal TotalMovement { get; }

        int TotalSteps { get; }

        #endregion
    }
}
