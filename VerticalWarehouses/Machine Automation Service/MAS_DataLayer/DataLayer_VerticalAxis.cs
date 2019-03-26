using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IVerticalAxis
    {
        #region Properties

        public bool HomingExecuted => this.GetBoolConfigurationValue((long)VerticalAxis.HomingExecuted, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingExitSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingExitSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public bool HomingSearchDirection => this.GetBoolConfigurationValue((long)VerticalAxis.HomingSearchDirection, (long)ConfigurationCategory.VerticalAxis);

        public decimal HomingSearchSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.HomingSearchSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal LowerBound => this.GetDecimalConfigurationValue((long)VerticalAxis.LowerBound, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxAcceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxDeceleration => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);

        public decimal MaxSpeed => this.GetDecimalConfigurationValue((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);

        public decimal Offset => this.GetDecimalConfigurationValue((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis);

        public decimal Resolution => this.GetDecimalConfigurationValue((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

        public decimal UpperBound => this.GetDecimalConfigurationValue((long)VerticalAxis.UpperBound, (long)ConfigurationCategory.VerticalAxis);

        #endregion
    }
}
