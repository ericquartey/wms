using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : ILoadFirstDrawer
    {
        #region Properties

        public Task<decimal> FeedRateLFD => this.GetDecimalConfigurationValueAsync((long)LoadFirstDrawer.FeedRate, (long)ConfigurationCategory.LoadFirstDrawer);

        #endregion
    }
}
