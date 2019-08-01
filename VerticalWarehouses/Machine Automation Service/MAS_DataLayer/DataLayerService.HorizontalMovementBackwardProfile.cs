using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementBackwardProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrectionBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.MovementCorrection, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P0SpeedV1Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P0SpeedV1, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P1SpeedV2Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P1SpeedV2, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P2SpeedV3Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P2SpeedV3, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P3SpeedV4Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P3SpeedV4, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4QuoteBackBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P4SpeedV5Back => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P4SpeedV5, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5AccelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5DecelerationBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5QuoteBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal P5SpeedBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.P5Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal TotalMovementBack => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.TotalMovement, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public int TotalStepsBack => this.GetIntegerConfigurationValue((long)HorizontalMovementBackwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        #endregion
    }
}
