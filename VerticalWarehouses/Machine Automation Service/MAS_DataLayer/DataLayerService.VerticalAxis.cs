using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalAxisDataLayer
    {
        #region Properties

        public Task<decimal> DepositOffset => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.DepositOffset, (long)ConfigurationCategory.VerticalAxis);

        public Task<bool> HomingExecuted => this.GetBoolConfigurationValueAsync((long)VerticalAxis.HomingExecuted, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingExitSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingExitSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<bool> HomingSearchDirection => this.GetBoolConfigurationValueAsync((long)VerticalAxis.HomingSearchDirection, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> HomingSearchSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.HomingSearchSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> LowerBound => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.LowerBound, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxEmptyAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxEmptyDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxEmptySpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxEmptySpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxFullAcceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxFullAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MaxFullDeceleration => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxFullDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> MinFullSpeed => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.MinFullSpeed, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> Offset => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> Resolution => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> TakingOffset => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.TakingOffset, (long)ConfigurationCategory.VerticalAxis);

        public Task<decimal> UpperBound => this.GetDecimalConfigurationValueAsync((long)VerticalAxis.UpperBound, (long)ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
