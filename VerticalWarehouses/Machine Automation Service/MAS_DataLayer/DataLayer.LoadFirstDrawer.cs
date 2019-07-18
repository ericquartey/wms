using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ILoadFirstDrawer
    {
        #region Properties

        public Task<decimal> FeedRateLFD => this.GetDecimalConfigurationValueAsync((long)LoadFirstDrawer.FeedRate, (long)ConfigurationCategory.LoadFirstDrawer);

        #endregion
    }
}
