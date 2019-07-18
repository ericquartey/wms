using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IHorizontalAxis
    {
        #region Properties

        public Task<decimal> AntiClockWiseRun => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.AntiClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> ClockWiseRun => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.ClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public Task<bool> HomingExecutedHA => this.GetBoolConfigurationValueAsync((long)HorizontalAxis.HomingExecuted, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxEmptyAccelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxEmptyDecelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxEmptySpeedHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxEmptySpeed, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxFullAccelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxFullAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxFullDecelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxFullDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxFullSpeed => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxFullSpeed, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> OffsetHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Offset, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> ResolutionHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
