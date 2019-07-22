using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IBayPositionControlDataLayer
    {
        #region Properties

        public Task<decimal> FeedRateBP => this.GetDecimalConfigurationValueAsync((long)BayPositionControl.FeedRate, (long)ConfigurationCategory.BayPositionControl);

        public Task<decimal> StepValueBP => this.GetDecimalConfigurationValueAsync((long)BayPositionControl.StepValue, (long)ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
