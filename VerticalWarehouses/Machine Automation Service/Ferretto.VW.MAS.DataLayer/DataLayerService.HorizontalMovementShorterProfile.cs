using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalMovementShorterProfileDataLayer
    {
        #region Properties

        public decimal MovementCorrectionShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.MovementCorrection, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P0AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P0Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P0DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P0Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P0QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P0Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P0SpeedV1Shorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P0SpeedV1, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P1AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P1Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P1DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P1Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P1QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P1Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P1SpeedV2Shorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P1SpeedV2, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P2AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P2Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P2DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P2Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P2QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P2Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P2SpeedV3Shorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P2SpeedV3, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P3AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P3Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P3DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P3Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P3QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P3Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P3SpeedV4Shorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P3SpeedV4, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P4AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P4Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P4DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P4Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P4QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P4Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P4SpeedV5Shorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P4SpeedV5, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P5AccelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P5Acceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P5DecelerationShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P5Deceleration, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P5QuoteShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P5Quote, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal P5SpeedShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.P5Speed, ConfigurationCategory.HorizontalMovementShorterProfile);

        public decimal TotalMovementShorter => this.GetDecimalConfigurationValue((long)HorizontalMovementShorterProfile.TotalMovement, ConfigurationCategory.HorizontalMovementShorterProfile);

        public int TotalStepsShorter => this.GetIntegerConfigurationValue((long)HorizontalMovementShorterProfile.TotalSteps, ConfigurationCategory.HorizontalMovementShorterProfile);

        #endregion
    }
}
