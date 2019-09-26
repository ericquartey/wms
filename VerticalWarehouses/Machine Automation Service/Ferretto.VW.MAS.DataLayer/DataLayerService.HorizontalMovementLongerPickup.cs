using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementLongerPickupDataLayer
    {
        #region Properties

        public decimal MovementCorrectionLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P0AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P0DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P0QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P0SpeedV1LongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P1AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P1DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P1QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P1SpeedV2LongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P2AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P2DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P2QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P2SpeedV3LongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P3AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P3DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P3QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P3SpeedV4LongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P4AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P4DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P4QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P4SpeedV5LongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P5AccelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P5DecelerationLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P5QuoteLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Quote, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal P5SpeedLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Speed, ConfigurationCategory.HorizontalMovementLongerPickup);

        public decimal TotalMovementLongerPickup => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.TotalMovement, ConfigurationCategory.HorizontalMovementLongerPickup);

        public int TotalStepsLongerPickup => this.GetIntegerConfigurationValue(HorizontalMovementLongerProfile.TotalSteps, ConfigurationCategory.HorizontalMovementLongerPickup);

        #endregion
    }
}
