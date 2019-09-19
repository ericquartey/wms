using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterHeightControlDataLayer
    {
        #region Properties

        public decimal FeedRateSH => this.GetDecimalConfigurationValue(ShutterHeightControl.FeedRate, ConfigurationCategory.ShutterHeightControl);

        public decimal RequiredTolerance => this.GetDecimalConfigurationValue(ShutterHeightControl.RequiredTolerance, ConfigurationCategory.ShutterHeightControl);

        #endregion
    }
}
