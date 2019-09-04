using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IProfileHeightCheckDataLayer
    {
        #region Properties

        public decimal FeedRateSH => this.GetDecimalConfigurationValue((long)ProfileHeightCheck.FeedRate, ConfigurationCategory.ProfileHeightCheck);

        public decimal RequiredTolerance => this.GetDecimalConfigurationValue((long)ProfileHeightCheck.RequiredTolerance, ConfigurationCategory.ProfileHeightCheck);

        #endregion
    }
}
