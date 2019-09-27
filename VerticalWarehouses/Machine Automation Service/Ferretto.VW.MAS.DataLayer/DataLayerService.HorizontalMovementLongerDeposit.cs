using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementLongerDepositDataLayer
    {
        #region Properties

        public decimal MovementCorrectionLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P0AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P0DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P0QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P0SpeedV1LongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P1AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P1DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P1QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P1SpeedV2LongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P2AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P2DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P2QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P2SpeedV3LongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P3AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P3DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P3QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P3SpeedV4LongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P4AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P4DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P4QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P4SpeedV5LongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P5AccelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P5DecelerationLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P5QuoteLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Quote, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal P5SpeedLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.P5Speed, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public decimal TotalMovementLongerDeposit => this.GetDecimalConfigurationValue(HorizontalMovementLongerProfile.TotalMovement, ConfigurationCategory.HorizontalMovementLongerDeposit);

        public int TotalStepsLongerDeposit => this.GetIntegerConfigurationValue(HorizontalMovementLongerProfile.TotalSteps, ConfigurationCategory.HorizontalMovementLongerDeposit);

        #endregion
    }
}
