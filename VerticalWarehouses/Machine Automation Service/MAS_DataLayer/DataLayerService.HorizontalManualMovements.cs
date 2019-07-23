using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IHorizontalManualMovements
    {
        #region Properties

        public decimal FeedRateHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

        public decimal InitialTargetPositionHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        public decimal RecoveryTargetPositionHM => this.GetDecimalConfigurationValue((long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        #endregion
    }
}
