using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IHorizontalMovementBackwardProfile
    {
        #region Properties

        public decimal InitialSpeedBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.InitialSpeed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step1AccDecBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step1AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step1PositionBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step1Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step1SpeedBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step1Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step2AccDecBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step2AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step2PositionBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step2Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step2SpeedBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step2Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step3AccDecBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step3AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step3PositionBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step3Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step3SpeedBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step3Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step4AccDecBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step4AccDec, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step4PositionBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step4Position, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public decimal Step4SpeedBP => this.GetDecimalConfigurationValue((long)HorizontalMovementBackwardProfile.Step4Speed, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        public int TotalStepsBP => this.GetIntegerConfigurationValue((long)HorizontalMovementBackwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementBackwardProfile);

        #endregion
    }
}
