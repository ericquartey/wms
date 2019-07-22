using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : IHorizontalMovementForwardProfile
    {
        #region Properties

        public Task<decimal> MovementCorrection => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.MovementCorrection, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P0Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P0Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P0Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P0Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P0Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P0Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P0SpeedV1 => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P0SpeedV1, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P1Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P1Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P1Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P1Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P1Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P1Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P1SpeedV2 => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P1SpeedV2, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P2Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P2Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P2Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P2Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P2Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P2Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P2SpeedV3 => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P2SpeedV3, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P3Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P3Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P3Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P3Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P3Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P3Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P3SpeedV4 => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P3SpeedV4, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P4Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P4Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P4Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P4Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P4Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P4Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P4SpeedV5 => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P4SpeedV5, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P5Acceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P5Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P5Deceleration => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P5Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P5Quote => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P5Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> P5Speed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.P5Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> TotalMovement => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.TotalMovement, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<int> TotalSteps => this.GetIntegerConfigurationValueAsync((long)HorizontalMovementForwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        #endregion
    }
}
