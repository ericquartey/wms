using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IHorizontalAxis
    {
        #region Properties

        public decimal AntiClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.AntiClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public decimal ClockWiseRun => this.GetDecimalConfigurationValue((long)HorizontalAxis.ClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public bool HomingExecutedHA => this.GetBoolConfigurationValue((long)HorizontalAxis.HomingExecuted, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxAccelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxDecelerationHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public decimal MaxSpeedHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.MaxSpeed, (long)ConfigurationCategory.HorizontalAxis);

        public decimal OffsetHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Offset, (long)ConfigurationCategory.HorizontalAxis);

        public decimal ResolutionHA => this.GetDecimalConfigurationValue((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
