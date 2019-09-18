using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalAxisDataLayer
    {
        #region Properties

        public decimal DepositOffset => this.GetDecimalConfigurationValue(VerticalAxis.DepositOffset, ConfigurationCategory.VerticalAxis);

        public bool HomingExecuted => this.GetBoolConfigurationValue(VerticalAxis.HomingExecuted, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitAcceleration => this.GetDecimalConfigurationValue(VerticalAxis.HomingExitAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitDeceleration => this.GetDecimalConfigurationValue(VerticalAxis.HomingExitDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitSpeed => this.GetDecimalConfigurationValue(VerticalAxis.HomingExitSpeed, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchAcceleration => this.GetDecimalConfigurationValue(VerticalAxis.HomingSearchAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchDeceleration => this.GetDecimalConfigurationValue(VerticalAxis.HomingSearchDeceleration, ConfigurationCategory.VerticalAxis);

        public bool HomingSearchDirection => this.GetBoolConfigurationValue(VerticalAxis.HomingSearchDirection, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchSpeed => this.GetDecimalConfigurationValue(VerticalAxis.HomingSearchSpeed, ConfigurationCategory.VerticalAxis);

        public decimal LowerBound => this.GetDecimalConfigurationValue(VerticalAxis.LowerBound, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyAcceleration => this.GetDecimalConfigurationValue(VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyDeceleration => this.GetDecimalConfigurationValue(VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptySpeed => this.GetDecimalConfigurationValue(VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);

        public decimal MaxFullAcceleration => this.GetDecimalConfigurationValue(VerticalAxis.MaxFullAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxFullDeceleration => this.GetDecimalConfigurationValue(VerticalAxis.MaxFullDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal MinFullSpeed => this.GetDecimalConfigurationValue(VerticalAxis.MinFullSpeed, ConfigurationCategory.VerticalAxis);

        public decimal Offset => this.GetDecimalConfigurationValue(VerticalAxis.Offset, ConfigurationCategory.VerticalAxis);

        public decimal Resolution
        {
            get => this.GetDecimalConfigurationValue(VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis);
            set => this.SetDecimalConfigurationValue((long)VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis, value);
        }

        public decimal TakingOffset => this.GetDecimalConfigurationValue(VerticalAxis.TakingOffset, ConfigurationCategory.VerticalAxis);

        public decimal UpperBound => this.GetDecimalConfigurationValue(VerticalAxis.UpperBound, ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
