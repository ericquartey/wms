using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementBackwardProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrectionBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0SpeedV1Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1SpeedV2Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2SpeedV3Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3SpeedV4Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4QuoteBackBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4SpeedV5Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Quote, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5SpeedBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Speed, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal TotalMovementBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.TotalMovement, ConfigurationCategory.HorizontalMovementBackwardProfile);

        public int TotalStepsBack => this.GetIntegerConfigurationValue((long)HorizontalMovementBackwardProfile.TotalSteps, ConfigurationCategory.HorizontalMovementBackwardProfile);

        #endregion
    }
}
