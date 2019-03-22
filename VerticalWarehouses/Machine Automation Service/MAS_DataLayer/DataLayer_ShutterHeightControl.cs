using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IShutterHeightControl
    {
        #region Properties

        public decimal FeedRateSH => this.GetDecimalConfigurationValue((long)ShutterHeightControl.FeedRate, (long)ConfigurationCategory.ShutterHeightControl);

        public decimal RequiredTolerance => this.GetDecimalConfigurationValue((long)ShutterHeightControl.RequiredTolerance, (long)ConfigurationCategory.ShutterHeightControl);

        #endregion
    }
}
