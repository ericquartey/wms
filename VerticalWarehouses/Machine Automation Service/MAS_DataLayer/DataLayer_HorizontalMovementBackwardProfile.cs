using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IHorizontalMovementBackwardProfile
    {
        #region Properties

        public Task<decimal> InitialSpeedBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.InitialSpeed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step1AccDecBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step1AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step1PositionBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step1Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step1SpeedBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step1Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step2AccDecBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step2AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step2PositionBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step2Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step2SpeedBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step2Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step3AccDecBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step3AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step3PositionBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step3Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step3SpeedBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step3Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step4AccDecBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step4AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step4PositionBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step4Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<decimal> Step4SpeedBP => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementBackwardProfile.Step4Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public Task<int> TotalStepsBP => this.GetIntegerConfigurationValueAsync((long)HorizontalMovementBackwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        #endregion
    }
}
