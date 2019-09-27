namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementShorterDepositDataLayer
    {
        #region Properties

        decimal MovementCorrectionShorterDeposit { get; }

        decimal P0AccelerationShorterDeposit { get; }

        decimal P0DecelerationShorterDeposit { get; }

        decimal P0QuoteShorterDeposit { get; }

        decimal P0SpeedV1ShorterDeposit { get; }

        decimal P1AccelerationShorterDeposit { get; }

        decimal P1DecelerationShorterDeposit { get; }

        decimal P1QuoteShorterDeposit { get; }

        decimal P1SpeedV2ShorterDeposit { get; }

        decimal P2AccelerationShorterDeposit { get; }

        decimal P2DecelerationShorterDeposit { get; }

        decimal P2QuoteShorterDeposit { get; }

        decimal P2SpeedV3ShorterDeposit { get; }

        decimal P3AccelerationShorterDeposit { get; }

        decimal P3DecelerationShorterDeposit { get; }

        decimal P3QuoteShorterDeposit { get; }

        decimal P3SpeedV4ShorterDeposit { get; }

        decimal P4AccelerationShorterDeposit { get; }

        decimal P4DecelerationShorterDeposit { get; }

        decimal P4QuoteShorterDeposit { get; }

        decimal P4SpeedV5ShorterDeposit { get; }

        decimal P5AccelerationShorterDeposit { get; }

        decimal P5DecelerationShorterDeposit { get; }

        decimal P5QuoteShorterDeposit { get; }

        decimal P5SpeedShorterDeposit { get; }

        decimal TotalMovementShorterDeposit { get; }

        int TotalStepsShorterDeposit { get; }

        #endregion
    }
}
