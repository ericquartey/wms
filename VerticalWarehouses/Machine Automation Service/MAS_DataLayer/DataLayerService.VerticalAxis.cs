using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalAxisDataLayer
    {
        #region Properties

        public decimal DepositOffset => this.GetDecimalConfigurationValue((long)VerticalAxis.DepositOffset, ConfigurationCategory.VerticalAxis);

        public bool HomingExecuted => this.GetBoolConfigurationValue((long)VerticalAxis.HomingExecuted, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingExitSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitSpeed, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchDeceleration, ConfigurationCategory.VerticalAxis);

        public bool HomingSearchDirection => this.GetBoolConfigurationValue((long)VerticalAxis.HomingSearchDirection, ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchSpeed, ConfigurationCategory.VerticalAxis);

        public decimal LowerBound => this.GetDecimalConfigurationValue((long)VerticalAxis.LowerBound, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptyDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxEmptySpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);

        public decimal MaxFullAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxFullAcceleration, ConfigurationCategory.VerticalAxis);

        public decimal MaxFullDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxFullDeceleration, ConfigurationCategory.VerticalAxis);

        public decimal MinFullSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.MinFullSpeed, ConfigurationCategory.VerticalAxis);

        public decimal Offset => this.GetDecimalConfigurationValue((long)VerticalAxis.Offset, ConfigurationCategory.VerticalAxis);

        public decimal Resolution
        {
            get => this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis);
            set => this.SetDecimalConfigurationValue((long)VerticalAxis.Resolution, ConfigurationCategory.VerticalAxis, value);
        }

        public decimal TakingOffset => this.GetDecimalConfigurationValue((long)VerticalAxis.TakingOffset, ConfigurationCategory.VerticalAxis);

        public decimal UpperBound => this.GetDecimalConfigurationValue((long)VerticalAxis.UpperBound, ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
