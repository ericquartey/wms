using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalManualMovementsDataLayer
    {
        #region Properties

        public decimal FeedRateAfterZero => this.GetDecimalConfigurationValue(VerticalManualMovements.FeedRateAfterZero, ConfigurationCategory.VerticalManualMovements);

        public decimal FeedRateVM => this.GetDecimalConfigurationValue(VerticalManualMovements.FeedRate, ConfigurationCategory.VerticalManualMovements);

        public decimal NegativeTargetDirection => this.GetDecimalConfigurationValue(VerticalManualMovements.NegativeTargetDirection, ConfigurationCategory.VerticalManualMovements);

        public decimal PositiveTargetDirection => this.GetDecimalConfigurationValue(VerticalManualMovements.PositiveTargetDirection, ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
