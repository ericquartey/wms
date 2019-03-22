using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IWeightControl
    {
        #region Properties

        public decimal FeedRateWC => this.GetDecimalConfigurationValue((long)WeightControl.FeedRate, (long)ConfigurationCategory.WeightControl);

        public decimal RequiredToleranceWC => this.GetDecimalConfigurationValue((long)WeightControl.RequiredTolerance, (long)ConfigurationCategory.WeightControl);

        public decimal TestRun => this.GetDecimalConfigurationValue((long)WeightControl.TestRun, (long)ConfigurationCategory.WeightControl);

        #endregion
    }
}
