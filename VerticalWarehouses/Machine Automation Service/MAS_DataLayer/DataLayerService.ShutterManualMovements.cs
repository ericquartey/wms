using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterManualMovementsDataLayer
    {
        #region Properties

        public decimal Acceleration => this.GetDecimalConfigurationValue((long)ShutterManualMovements.Acceleration, ConfigurationCategory.ShutterManualMovements);

        public decimal Deceleration => this.GetDecimalConfigurationValue((long)ShutterManualMovements.Deceleration, ConfigurationCategory.ShutterManualMovements);

        public decimal FeedRateSM => this.GetDecimalConfigurationValue((long)ShutterManualMovements.FeedRate, ConfigurationCategory.ShutterManualMovements);

        public decimal MaxSpeed => this.GetDecimalConfigurationValue((long)ShutterManualMovements.MaxSpeed, ConfigurationCategory.ShutterManualMovements);

        #endregion
    }
}
