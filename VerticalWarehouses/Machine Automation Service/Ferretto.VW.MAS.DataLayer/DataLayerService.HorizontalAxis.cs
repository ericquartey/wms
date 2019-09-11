using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalAxisDataLayer
    {
        #region Properties

        public decimal AntiClockWiseRun => this.GetDecimalConfigurationValue(HorizontalAxis.AntiClockWiseRun, ConfigurationCategory.HorizontalAxis);

        public decimal ClockWiseRun => this.GetDecimalConfigurationValue(HorizontalAxis.ClockWiseRun, ConfigurationCategory.HorizontalAxis);

        public bool HomingExecutedHA => this.GetBoolConfigurationValue(HorizontalAxis.HomingExecuted, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyAccelerationHA => this.GetDecimalConfigurationValue(HorizontalAxis.MaxEmptyAcceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyDecelerationHA => this.GetDecimalConfigurationValue(HorizontalAxis.MaxEmptyDeceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptySpeedHA => this.GetDecimalConfigurationValue(HorizontalAxis.MaxEmptySpeed, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullAccelerationHA => this.GetDecimalConfigurationValue(HorizontalAxis.MaxFullAcceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullDecelerationHA => this.GetDecimalConfigurationValue(HorizontalAxis.MaxFullDeceleration, ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullSpeed => this.GetDecimalConfigurationValue(HorizontalAxis.MaxFullSpeed, ConfigurationCategory.HorizontalAxis);

        public decimal OffsetHA => this.GetDecimalConfigurationValue(HorizontalAxis.Offset, ConfigurationCategory.HorizontalAxis);

        public decimal ResolutionHA => this.GetDecimalConfigurationValue(HorizontalAxis.Resolution, ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
