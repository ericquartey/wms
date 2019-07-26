using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterManualMovementsDataLayer
    {
        #region Properties

        public decimal Acceleration => this.GetDecimalConfigurationValue((long)ShutterManualMovements.Acceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public decimal Deceleration => this.GetDecimalConfigurationValue((long)ShutterManualMovements.Deceleration, (long)ConfigurationCategory.ShutterManualMovements);

        public decimal FeedRateSM => this.GetDecimalConfigurationValue((long)ShutterManualMovements.FeedRate, (long)ConfigurationCategory.ShutterManualMovements);

        public decimal MaxSpeed => this.GetDecimalConfigurationValue((long)ShutterManualMovements.MaxSpeed, (long)ConfigurationCategory.ShutterManualMovements);

        #endregion
    }
}
