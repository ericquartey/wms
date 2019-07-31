using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalManualMovementsDataLayer
    {
        #region Properties

        public decimal FeedRateHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.FeedRate, ConfigurationCategory.HorizontalManualMovements);

        public decimal InitialTargetPositionHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.InitialTargetPosition, ConfigurationCategory.HorizontalManualMovements);

        public decimal RecoveryTargetPositionHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.RecoveryTargetPosition, ConfigurationCategory.HorizontalManualMovements);

        #endregion
    }
}
