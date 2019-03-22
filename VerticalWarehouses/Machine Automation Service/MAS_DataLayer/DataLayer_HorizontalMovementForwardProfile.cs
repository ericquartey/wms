using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IHorizontalMovementForwardProfile
    {
        #region Properties

        public decimal InitialSpeed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.InitialSpeed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step1AccDec => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step1AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step1Position => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step1Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step1Speed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step1Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step2AccDec => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step2AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step2Position => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step2Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step2Speed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step2Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step3AccDec => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step3AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step3Position => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step3Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step3Speed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step3Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step4AccDec => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step4AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step4Position => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step4Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal Step4Speed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.Step4Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public int TotalSteps => this.GetIntegerConfigurationValue((long)HorizontalMovementForwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        #endregion
    }
}
