using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IShutterHeightControl
    {
        #region Properties

        public Task<decimal> FeedRateSH => this.GetDecimalConfigurationValueAsync((long)ShutterHeightControl.FeedRate, (long)ConfigurationCategory.ShutterHeightControl);

        public Task<decimal> RequiredTolerance => this.GetDecimalConfigurationValueAsync((long)ShutterHeightControl.RequiredTolerance, (long)ConfigurationCategory.ShutterHeightControl);

        #endregion
    }
}
