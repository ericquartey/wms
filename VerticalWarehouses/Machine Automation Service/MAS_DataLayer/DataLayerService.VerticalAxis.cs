using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalAxis
    {
        #region Properties

        public decimal DepositOffset => this.GetDecimalConfigurationValue((long)VerticalAxis.DepositOffset, (long)ConfigurationCategory.VerticalAxis);

        public bool HomingExecuted => this.GetBoolConfigurationValue((long)VerticalAxis.HomingExecuted, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public bool HomingSearchDirection => this.GetBoolConfigurationValue((long)VerticalAxis.HomingSearchDirection, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal LowerBound => this.GetDecimalConfigurationValue((long)VerticalAxis.LowerBound, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyAcceleration => this.GetDecimalConfigurationValue ((long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyDeceleration => this.GetDecimalConfigurationValue ((long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptySpeed => this.GetDecimalConfigurationValue ((long)VerticalAxis.MaxEmptySpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxFullAcceleration => this.GetDecimalConfigurationValue ((long)VerticalAxis.MaxFullAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxFullDeceleration => this.GetDecimalConfigurationValue ((long)VerticalAxis.MaxFullDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MinFullSpeed => this.GetDecimalConfigurationValue ((long)VerticalAxis.MinFullSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal Offset => this.GetDecimalConfigurationValue((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis);

        public decimal Resolution => this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

        public decimal TakingOffset => this.GetDecimalConfigurationValue((long)VerticalAxis.TakingOffset, (long)ConfigurationCategory.VerticalAxis);

        public decimal UpperBound => this.GetDecimalConfigurationValue((long)VerticalAxis.UpperBound, (long)ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
