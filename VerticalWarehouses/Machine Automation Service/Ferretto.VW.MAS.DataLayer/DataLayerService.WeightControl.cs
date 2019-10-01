using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IWeightControlDataLayer
    {
        #region Properties

        public decimal FeedRateWC => this.GetDecimalConfigurationValue(WeightControl.FeedRate, ConfigurationCategory.WeightControl);

        public decimal RequiredToleranceWC => this.GetDecimalConfigurationValue(WeightControl.RequiredTolerance, ConfigurationCategory.WeightControl);

        public decimal TestRun => this.GetDecimalConfigurationValue(WeightControl.TestRun, ConfigurationCategory.WeightControl);

        #endregion
    }
}
