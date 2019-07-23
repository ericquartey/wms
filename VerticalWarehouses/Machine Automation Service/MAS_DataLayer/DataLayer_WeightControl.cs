using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
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
