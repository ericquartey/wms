using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ILoadFirstDrawer
    {
        #region Properties

        public decimal FeedRateLFD => this.GetDecimalConfigurationValue((long)LoadFirstDrawer.FeedRate, (long)ConfigurationCategory.LoadFirstDrawer);

        #endregion
    }
}
