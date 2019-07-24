using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalAxis
    {
        #region Properties

        public decimal AntiClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.AntiClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public decimal ClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.ClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public bool HomingExecutedHA => this.GetBoolConfigurationValue((long)HorizontalAxis.HomingExecuted, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyAccelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptyDecelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxEmptySpeedHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxEmptySpeed, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullAccelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullDecelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxFullSpeed => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxFullSpeed, (long)ConfigurationCategory.HorizontalAxis);

        public decimal OffsetHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Offset, (long)ConfigurationCategory.HorizontalAxis);

        public decimal ResolutionHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
