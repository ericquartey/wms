using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalAxisDataLayer
    {
        #region Properties

        public decimal AntiClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.AntiClockWiseRun, ConfigurationCategory.HorizontalAxis);

        public decimal ClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.ClockWiseRun, ConfigurationCategory.HorizontalAxis);

        public bool HomingExecutedHA => this.GetBoolConfigurationValue((long)HorizontalAxis.HomingExecuted, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyAccelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptyAcceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyDecelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptyDeceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptySpeedHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptySpeed, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullAccelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullAcceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullDecelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullDeceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullSpeed => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullSpeed, ConfigurationCategory.HorizontalAxis);

        public decimal OffsetHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Offset, ConfigurationCategory.HorizontalAxis);

        public decimal ResolutionHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
