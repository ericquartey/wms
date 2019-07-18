using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IHorizontalAxis
    {
        #region Properties

        public Task<decimal> AntiClockWiseRun => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.AntiClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> ClockWiseRun => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.ClockWiseRun, (long)ConfigurationCategory.HorizontalAxis);

        public Task<bool> HomingExecutedHA => this.GetBoolConfigurationValueAsync((long)HorizontalAxis.HomingExecuted, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxAccelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxAcceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxDecelerationHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxDeceleration, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> MaxSpeedHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxSpeed, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> OffsetHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Offset, (long)ConfigurationCategory.HorizontalAxis);

        public Task<decimal> ResolutionHA => this.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

        #endregion
    }
}
