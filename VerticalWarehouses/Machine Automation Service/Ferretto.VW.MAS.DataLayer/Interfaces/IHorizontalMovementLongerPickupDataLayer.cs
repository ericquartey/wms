namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalMovementLongerPickupDataLayer
    {
        #region Properties

        decimal MovementCorrectionLongerPickup { get; }

        decimal P0AccelerationLongerPickup { get; }

        decimal P0DecelerationLongerPickup { get; }

        decimal P0QuoteLongerPickup { get; }

        decimal P0SpeedV1LongerPickup { get; }

        decimal P1AccelerationLongerPickup { get; }

        decimal P1DecelerationLongerPickup { get; }

        decimal P1QuoteLongerPickup { get; }

        decimal P1SpeedV2LongerPickup { get; }

        decimal P2AccelerationLongerPickup { get; }

        decimal P2DecelerationLongerPickup { get; }

        decimal P2QuoteLongerPickup { get; }

        decimal P2SpeedV3LongerPickup { get; }

        decimal P3AccelerationLongerPickup { get; }

        decimal P3DecelerationLongerPickup { get; }

        decimal P3QuoteLongerPickup { get; }

        decimal P3SpeedV4LongerPickup { get; }

        decimal P4AccelerationLongerPickup { get; }

        decimal P4DecelerationLongerPickup { get; }

        decimal P4QuoteLongerPickup { get; }

        decimal P4SpeedV5LongerPickup { get; }

        decimal P5AccelerationLongerPickup { get; }

        decimal P5DecelerationLongerPickup { get; }

        decimal P5QuoteLongerPickup { get; }

        decimal P5SpeedLongerPickup { get; }

        decimal TotalMovementLongerPickup { get; }

        int TotalStepsLongerPickup { get; }

        #endregion
    }
}
