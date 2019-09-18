namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementLongerProfileDataLayer
    {
        #region Properties

        decimal MovementCorrectionLonger { get; }

        decimal P0AccelerationLonger { get; }

        decimal P0DecelerationLonger { get; }

        decimal P0QuoteLonger { get; }

        decimal P0SpeedV1Longer { get; }

        decimal P1AccelerationLonger { get; }

        decimal P1DecelerationLonger { get; }

        decimal P1QuoteLonger { get; }

        decimal P1SpeedV2Longer { get; }

        decimal P2AccelerationLonger { get; }

        decimal P2DecelerationLonger { get; }

        decimal P2QuoteLonger { get; }

        decimal P2SpeedV3Longer { get; }

        decimal P3AccelerationLonger { get; }

        decimal P3DecelerationLonger { get; }

        decimal P3QuoteLonger { get; }

        decimal P3SpeedV4Longer { get; }

        decimal P4AccelerationLonger { get; }

        decimal P4DecelerationLonger { get; }

        decimal P4QuoteLonger { get; }

        decimal P4SpeedV5Longer { get; }

        decimal P5AccelerationLonger { get; }

        decimal P5DecelerationLonger { get; }

        decimal P5QuoteLonger { get; }

        decimal P5SpeedLonger { get; }

        decimal TotalMovementLonger { get; }

        int TotalStepsLonger { get; }

        #endregion
    }
}
