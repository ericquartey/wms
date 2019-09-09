using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementLongerProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrectionLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P0Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P0SpeedV1Longer => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P1Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P1SpeedV2Longer => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P2Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P2SpeedV3Longer => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P3Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P3SpeedV4Longer => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P4Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P4SpeedV5Longer => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5AccelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5DecelerationLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5QuoteLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P5Quote, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal P5SpeedLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.P5Speed, ConfigurationCategory.HorizontalMovementLongerProfile);

        public decimal TotalMovementLonger => this.GetDecimalConfigurationValue((long)HorizontalMovementLongerProfile.TotalMovement, ConfigurationCategory.HorizontalMovementLongerProfile);

        public int TotalStepsLonger => this.GetIntegerConfigurationValue((long)HorizontalMovementLongerProfile.TotalSteps, ConfigurationCategory.HorizontalMovementLongerProfile);

        #endregion
    }
}
