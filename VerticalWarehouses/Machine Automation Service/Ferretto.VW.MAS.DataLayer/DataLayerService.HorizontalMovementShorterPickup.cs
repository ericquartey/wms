using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementShorterPickupDataLayer
    {
        #region Properties

        public decimal MovementCorrectionShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P0AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P0DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P0QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P0SpeedV1ShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P1AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P1DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P1QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P1SpeedV2ShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P2AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P2DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P2QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P2SpeedV3ShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P3AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P3DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P3QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P3SpeedV4ShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P4AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P4DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P4QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P4SpeedV5ShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P5AccelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P5DecelerationShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P5QuoteShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Quote, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal P5SpeedShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Speed, ConfigurationCategory.HorizontalMovementShorterPickup);

        public decimal TotalMovementShorterPickup => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.TotalMovement, ConfigurationCategory.HorizontalMovementShorterPickup);

        public int TotalStepsShorterPickup => this.GetIntegerConfigurationValue(HorizontalMovementShorterProfile.TotalSteps, ConfigurationCategory.HorizontalMovementShorterPickup);

        #endregion
    }
}
