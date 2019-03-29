using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IVerticalAxis
    {
        #region Properties

        public Task<bool> HomingExecuted => this.GetBoolConfigurationValueAsync((long)VerticalAxis.HomingExecuted, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<bool> HomingSearchDirection => this.GetBoolConfigurationValueAsync((long)VerticalAxis.HomingSearchDirection, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> LowerBound => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.LowerBound, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> Offset => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> Resolution => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> UpperBound => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.UpperBound, (long)ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
