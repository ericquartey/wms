using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IShutterManualMovements
    {
        #region Properties

        public Task<decimal> Acceleration => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.Acceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> Deceleration => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.Deceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> FeedRateSM => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.FeedRate, (long)ConfigurationCategory.ShutterManualMovements);

        public Task<decimal> MaxSpeed => this.GetDecimalConfigurationValueAsync((long)ShutterManualMovements.MaxSpeed, (long)ConfigurationCategory.ShutterManualMovements);

        #endregion
    }
}
