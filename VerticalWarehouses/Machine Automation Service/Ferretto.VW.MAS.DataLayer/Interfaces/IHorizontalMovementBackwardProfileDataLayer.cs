namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementShorterProfileDataLayer
    {
        #region Properties

        decimal MovementCorrectionShorter { get; }

        decimal P0AccelerationShorter { get; }

        decimal P0DecelerationShorter { get; }

        decimal P0QuoteShorter { get; }

        decimal P0SpeedV1Shorter { get; }

        decimal P1AccelerationShorter { get; }

        decimal P1DecelerationShorter { get; }

        decimal P1QuoteShorter { get; }

        decimal P1SpeedV2Shorter { get; }

        decimal P2AccelerationShorter { get; }

        decimal P2DecelerationShorter { get; }

        decimal P2QuoteShorter { get; }

        decimal P2SpeedV3Shorter { get; }

        decimal P3AccelerationShorter { get; }

        decimal P3DecelerationShorter { get; }

        decimal P3QuoteShorter { get; }

        decimal P3SpeedV4Shorter { get; }

        decimal P4AccelerationShorter { get; }

        decimal P4DecelerationShorter { get; }

        decimal P4QuoteShorter { get; }

        decimal P4SpeedV5Shorter { get; }

        decimal P5AccelerationShorter { get; }

        decimal P5DecelerationShorter { get; }

        decimal P5QuoteShorter { get; }

        decimal P5SpeedShorter { get; }

        decimal TotalMovementShorter { get; }

        int TotalStepsShorter { get; }

        #endregion
    }
}
