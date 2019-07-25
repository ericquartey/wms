using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IWeightControlDataLayer
    {
        #region Properties

        public decimal FeedRateWC => this.GetDecimalConfigurationValue((long)WeightControl.FeedRate, (long)ConfigurationCategory.WeightControl);

        public decimal RequiredToleranceWC => this.GetDecimalConfigurationValue((long)WeightControl.RequiredTolerance, (long)ConfigurationCategory.WeightControl);

        public decimal TestRun => this.GetDecimalConfigurationValue((long)WeightControl.TestRun, (long)ConfigurationCategory.WeightControl);

        #endregion
    }
}
