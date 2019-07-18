using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IHorizontalMovementForwardProfile
    {
        #region Properties

        public Task<decimal> InitialSpeed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.InitialSpeed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step1AccDec => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step1AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step1Position => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step1Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step1Speed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step1Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step2AccDec => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step2AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step2Position => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step2Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step2Speed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step2Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step3AccDec => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step3AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step3Position => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step3Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step3Speed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step3Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step4AccDec => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step4AccDec, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step4Position => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step4Position, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<decimal> Step4Speed => this.GetDecimalConfigurationValueAsync((long)HorizontalMovementForwardProfile.Step4Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public Task<int> TotalSteps => this.GetIntegerConfigurationValueAsync((long)HorizontalMovementForwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        #endregion
    }
}
