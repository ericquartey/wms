using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementShorterDepositDataLayer
    {
        #region Properties

        public decimal MovementCorrectionShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P0AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P0DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P0QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P0SpeedV1ShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P1AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P1DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P1QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P1SpeedV2ShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P2AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P2DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P2QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P2SpeedV3ShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P3AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P3DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P3QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P3SpeedV4ShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P4AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P4DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P4QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P4SpeedV5ShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P5AccelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P5DecelerationShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P5QuoteShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Quote, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal P5SpeedShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.P5Speed, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public decimal TotalMovementShorterDeposit => this.GetDecimalConfigurationValue(HorizontalMovementShorterProfile.TotalMovement, ConfigurationCategory.HorizontalMovementShorterDeposit);

        public int TotalStepsShorterDeposit => this.GetIntegerConfigurationValue(HorizontalMovementShorterProfile.TotalSteps, ConfigurationCategory.HorizontalMovementShorterDeposit);

        #endregion
    }
}
