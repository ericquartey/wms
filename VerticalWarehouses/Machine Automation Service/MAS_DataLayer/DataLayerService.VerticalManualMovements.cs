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

        public decimal NegativeTargetPosition => this.GetDecimalConfigurationValue((long)VerticalManualMovements.NegativeTargetPosition, ConfigurationCategory.VerticalManualMovements);

        public decimal PositiveTargetPosition => this.GetDecimalConfigurationValue((long)VerticalManualMovements.PositiveTargetPosition, ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
