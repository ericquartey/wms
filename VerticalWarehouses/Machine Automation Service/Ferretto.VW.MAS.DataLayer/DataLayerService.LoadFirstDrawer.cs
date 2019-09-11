using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ILoadFirstDrawerDataLayer
    {
        #region Properties

        public decimal FeedRateLFD => this.GetDecimalConfigurationValue(LoadFirstDrawer.FeedRate, ConfigurationCategory.LoadFirstDrawer);

        #endregion
    }
}
