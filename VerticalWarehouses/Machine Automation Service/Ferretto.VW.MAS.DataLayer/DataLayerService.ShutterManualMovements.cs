using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IShutterManualMovementsDataLayer
    {
        #region Properties

        public decimal Acceleration => this.GetDecimalConfigurationValue(ShutterManualMovements.Acceleration, ConfigurationCategory.ShutterManualMovements);

        public decimal Deceleration => this.GetDecimalConfigurationValue(ShutterManualMovements.Deceleration, ConfigurationCategory.ShutterManualMovements);

        public decimal FeedRateSM => this.GetDecimalConfigurationValue(ShutterManualMovements.FeedRate, ConfigurationCategory.ShutterManualMovements);

        public decimal HigherDistance => this.GetDecimalConfigurationValue(ShutterManualMovements.HigherDistance, ConfigurationCategory.ShutterManualMovements);

        public decimal HighSpeedDurationClose => this.GetDecimalConfigurationValue(ShutterManualMovements.HighSpeedDurationClose, ConfigurationCategory.ShutterManualMovements);

        public decimal HighSpeedDurationOpen => this.GetDecimalConfigurationValue(ShutterManualMovements.HighSpeedDurationOpen, ConfigurationCategory.ShutterManualMovements);

        public decimal LowerDistance => this.GetDecimalConfigurationValue(ShutterManualMovements.LowerDistance, ConfigurationCategory.ShutterManualMovements);

        public decimal MaxSpeed => this.GetDecimalConfigurationValue(ShutterManualMovements.MaxSpeed, ConfigurationCategory.ShutterManualMovements);

        public decimal MinSpeed => this.GetDecimalConfigurationValue(ShutterManualMovements.MinSpeed, ConfigurationCategory.ShutterManualMovements);

        #endregion
    }
}
