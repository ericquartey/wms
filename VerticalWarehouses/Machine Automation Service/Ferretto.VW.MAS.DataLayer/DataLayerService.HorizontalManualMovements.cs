using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalManualMovementsDataLayer
    {
        #region Properties

        public decimal FeedRateHM => this.GetDecimalConfigurationValue(HorizontalManualMovements.FeedRate, ConfigurationCategory.HorizontalManualMovements);

        public decimal InitialTargetPositionHM => this.GetDecimalConfigurationValue(HorizontalManualMovements.InitialTargetPosition, ConfigurationCategory.HorizontalManualMovements);

        public decimal RecoveryTargetPositionHM => this.GetDecimalConfigurationValue(HorizontalManualMovements.RecoveryTargetPosition, ConfigurationCategory.HorizontalManualMovements);

        #endregion
    }
}
