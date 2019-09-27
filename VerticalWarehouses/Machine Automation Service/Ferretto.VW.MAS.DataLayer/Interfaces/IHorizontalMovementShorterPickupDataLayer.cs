namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementShorterPickupDataLayer
    {
        #region Properties

        decimal MovementCorrectionShorterPickup { get; }

        decimal P0AccelerationShorterPickup { get; }

        decimal P0DecelerationShorterPickup { get; }

        decimal P0QuoteShorterPickup { get; }

        decimal P0SpeedV1ShorterPickup { get; }

        decimal P1AccelerationShorterPickup { get; }

        decimal P1DecelerationShorterPickup { get; }

        decimal P1QuoteShorterPickup { get; }

        decimal P1SpeedV2ShorterPickup { get; }

        decimal P2AccelerationShorterPickup { get; }

        decimal P2DecelerationShorterPickup { get; }

        decimal P2QuoteShorterPickup { get; }

        decimal P2SpeedV3ShorterPickup { get; }

        decimal P3AccelerationShorterPickup { get; }

        decimal P3DecelerationShorterPickup { get; }

        decimal P3QuoteShorterPickup { get; }

        decimal P3SpeedV4ShorterPickup { get; }

        decimal P4AccelerationShorterPickup { get; }

        decimal P4DecelerationShorterPickup { get; }

        decimal P4QuoteShorterPickup { get; }

        decimal P4SpeedV5ShorterPickup { get; }

        decimal P5AccelerationShorterPickup { get; }

        decimal P5DecelerationShorterPickup { get; }

        decimal P5QuoteShorterPickup { get; }

        decimal P5SpeedShorterPickup { get; }

        decimal TotalMovementShorterPickup { get; }

        int TotalStepsShorterPickup { get; }

        #endregion
    }
}
