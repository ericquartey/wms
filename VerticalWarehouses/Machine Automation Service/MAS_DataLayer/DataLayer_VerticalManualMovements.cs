using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IVerticalManualMovements
    {
        #region Properties

        public decimal FeedRateVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

        public decimal InitialTargetPositionVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);

        public decimal RecoveryTargetPositionVM => this.GetDecimalConfigurationValue((long)VerticalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
