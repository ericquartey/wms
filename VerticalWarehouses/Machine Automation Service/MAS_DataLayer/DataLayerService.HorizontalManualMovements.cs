using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : IHorizontalManualMovements
    {
        #region Properties

        public Task<decimal> FeedRateHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

        public Task<decimal> InitialTargetPositionHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        public Task<decimal> RecoveryTargetPositionHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        #endregion
    }
}
