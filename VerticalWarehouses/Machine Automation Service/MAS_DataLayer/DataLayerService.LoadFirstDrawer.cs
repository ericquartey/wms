using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ILoadFirstDrawer
    {
        #region Properties

        public decimal FeedRateLFD => this.GetDecimalConfigurationValue((long)LoadFirstDrawer.FeedRate, (long)ConfigurationCategory.LoadFirstDrawer);

        #endregion
    }
}
