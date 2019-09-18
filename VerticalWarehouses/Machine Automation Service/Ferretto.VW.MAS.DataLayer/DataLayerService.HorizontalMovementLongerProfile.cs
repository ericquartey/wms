using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementLongerProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrectionLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0SpeedV1Longer => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1SpeedV2Longer => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2SpeedV3Longer => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3SpeedV4Longer => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4SpeedV5Longer => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5AccelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5DecelerationLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5QuoteLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5SpeedLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Speed, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal TotalMovementLonger => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.TotalMovement, ConfigurationCategory.HorizontalMovementLongerProfile);

        public int TotalStepsLonger => this.GetIntegerConfigurationValue(HorizontalMovementLongerProfile.TotalSteps, ConfigurationCategory.HorizontalMovementLongerProfile);

        #endregion
    }
}
