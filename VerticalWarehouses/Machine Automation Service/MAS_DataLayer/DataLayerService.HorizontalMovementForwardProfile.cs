using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementForwardProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrection => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.MovementCorrection, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P0Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P0Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P0Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P0Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P0Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P0Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P0SpeedV1 => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P0SpeedV1, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P1Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P1Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P1Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P1Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P1Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P1Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P1SpeedV2 => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P1SpeedV2, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P2Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P2Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P2Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P2Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P2Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P2Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P2SpeedV3 => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P2SpeedV3, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P3Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P3Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P3Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P3Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P3Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P3Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P3SpeedV4 => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P3SpeedV4, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P4Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P4Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P4Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P4Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P4Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P4Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P4SpeedV5 => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P4SpeedV5, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P5Acceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P5Acceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P5Deceleration => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P5Deceleration, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P5Quote => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P5Quote, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal P5Speed => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.P5Speed, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public decimal TotalMovement => this.GetDecimalConfigurationValue((long)HorizontalMovementForwardProfile.TotalMovement, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        public int TotalSteps => this.GetIntegerConfigurationValue((long)HorizontalMovementForwardProfile.TotalSteps, (long)ConfigurationCategory.HorizontalMovementForwardProfile);

        #endregion
    }
}
