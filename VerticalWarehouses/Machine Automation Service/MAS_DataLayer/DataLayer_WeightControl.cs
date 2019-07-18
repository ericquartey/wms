using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IWeightControl
    {
        #region Properties

        public Task<decimal> FeedRateWC => this.GetDecimalConfigurationValueAsync((long)WeightControl.FeedRate, (long)ConfigurationCategory.WeightControl);

        public Task<decimal> RequiredToleranceWC => this.GetDecimalConfigurationValueAsync((long)WeightControl.RequiredTolerance, (long)ConfigurationCategory.WeightControl);

        public Task<decimal> TestRun => this.GetDecimalConfigurationValueAsync((long)WeightControl.TestRun, (long)ConfigurationCategory.WeightControl);

        #endregion
    }
}
