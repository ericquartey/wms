using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementBackwardProfileDataLayer
    {
        #region Properties

        public Task<decimal> MovementCorrectionBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.MovementCorrection, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P0AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P0Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P0DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P0Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P0QuoteBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P0Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P0SpeedV1Back => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P0SpeedV1, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P1AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P1Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P1DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P1Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P1QuoteBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P1Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P1SpeedV2Back => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P1SpeedV2, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P2AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P2Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P2DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P2Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P2QuoteBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P2Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P2SpeedV3Back => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P2SpeedV3, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P3AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P3Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P3DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P3Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P3QuoteBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P3Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P3SpeedV4Back => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P3SpeedV4, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P4AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P4Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P4DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P4Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P4QuoteBackBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P4Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P4SpeedV5Back => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P4SpeedV5, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P5AccelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P5Acceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P5DecelerationBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P5Deceleration, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P5QuoteBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P5Quote, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> P5SpeedBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.P5Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> TotalMovementBack => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.TotalMovement, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<int> TotalStepsBack => this.GetIntegerConfigurationValueAsync((long)HorizontalMovementBackwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        #endregion
    }
}
