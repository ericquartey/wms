using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterHeightControl
    {
        #region Properties

        public decimal FeedRateSH => this.GetDecimalConfigurationValue((long)ShutterHeightControl.FeedRate, (long)ConfigurationCategory.ShutterHeightControl);

        public decimal RequiredTolerance => this.GetDecimalConfigurationValue((long)ShutterHeightControl.RequiredTolerance, (long)ConfigurationCategory.ShutterHeightControl);

        #endregion
    }
}
