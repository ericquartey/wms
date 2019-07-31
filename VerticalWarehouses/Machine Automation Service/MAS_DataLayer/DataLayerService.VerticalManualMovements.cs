using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalManualMovementsDataLayer
    {
        #region Properties

        public decimal FeedRateVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.FeedRate, ConfigurationCategory.VerticalManualMovements);

        public decimal InitialTargetPositionVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.InitialTargetPosition, ConfigurationCategory.VerticalManualMovements);

        public decimal RecoveryTargetPositionVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.RecoveryTargetPosition, ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
