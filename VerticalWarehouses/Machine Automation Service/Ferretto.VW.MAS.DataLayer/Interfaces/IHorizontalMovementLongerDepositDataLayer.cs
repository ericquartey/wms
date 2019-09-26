namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementLongerDepositDataLayer
    {
        #region Properties

        decimal MovementCorrectionLongerDeposit { get; }

        decimal P0AccelerationLongerDeposit { get; }

        decimal P0DecelerationLongerDeposit { get; }

        decimal P0QuoteLongerDeposit { get; }

        decimal P0SpeedV1LongerDeposit { get; }

        decimal P1AccelerationLongerDeposit { get; }

        decimal P1DecelerationLongerDeposit { get; }

        decimal P1QuoteLongerDeposit { get; }

        decimal P1SpeedV2LongerDeposit { get; }

        decimal P2AccelerationLongerDeposit { get; }

        decimal P2DecelerationLongerDeposit { get; }

        decimal P2QuoteLongerDeposit { get; }

        decimal P2SpeedV3LongerDeposit { get; }

        decimal P3AccelerationLongerDeposit { get; }

        decimal P3DecelerationLongerDeposit { get; }

        decimal P3QuoteLongerDeposit { get; }

        decimal P3SpeedV4LongerDeposit { get; }

        decimal P4AccelerationLongerDeposit { get; }

        decimal P4DecelerationLongerDeposit { get; }

        decimal P4QuoteLongerDeposit { get; }

        decimal P4SpeedV5LongerDeposit { get; }

        decimal P5AccelerationLongerDeposit { get; }

        decimal P5DecelerationLongerDeposit { get; }

        decimal P5QuoteLongerDeposit { get; }

        decimal P5SpeedLongerDeposit { get; }

        decimal TotalMovementLongerDeposit { get; }

        int TotalStepsLongerDeposit { get; }

        #endregion
    }
}
