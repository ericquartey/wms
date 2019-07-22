using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : IWeightControl
    {
        #region Properties

        public Task<decimal> FeedRateWC => this.GetDecimalConfigurationValueAsync((long)WeightControl.FeedRate, (long)ConfigurationCategory.WeightControl);

        public Task<decimal> RequiredToleranceWC => this.GetDecimalConfigurationValueAsync((long)WeightControl.RequiredTolerance, (long)ConfigurationCategory.WeightControl);

        public Task<decimal> TestRun => this.GetDecimalConfigurationValueAsync((long)WeightControl.TestRun, (long)ConfigurationCategory.WeightControl);

        #endregion
    }
}
