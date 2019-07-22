using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterManualMovementsDataLayer
    {
        #region Properties

        public Task<decimal> Acceleration => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.Acceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> Deceleration => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.Deceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> FeedRateSM => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.FeedRate, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> MaxSpeed => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.MaxSpeed, (long)ConfigurationCategory.ShutterManualMovements);

        #endregion
    }
}
