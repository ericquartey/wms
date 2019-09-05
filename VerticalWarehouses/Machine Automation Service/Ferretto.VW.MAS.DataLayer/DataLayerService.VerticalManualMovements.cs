using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IVerticalManualMovementsDataLayer
    {
        #region Properties

        public decimal FeedRateAfterZero => this.GetDecimalConfigurationValue((long)VerticalManualMovements.FeedRateAfterZero, ConfigurationCategory.VerticalManualMovements);

        public decimal FeedRateVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.FeedRate, ConfigurationCategory.VerticalManualMovements);

        public decimal NegativeTargetDirection => this.GetDecimalConfigurationValue((long)VerticalManualMovements.NegativeTargetDirection, ConfigurationCategory.VerticalManualMovements);

        public decimal PositiveTargetDirection => this.GetDecimalConfigurationValue((long)VerticalManualMovements.PositiveTargetDirection, ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
