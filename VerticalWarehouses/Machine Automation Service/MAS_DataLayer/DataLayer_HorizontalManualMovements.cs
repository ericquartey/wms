using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IHorizontalManualMovements
    {
        #region Properties

        public Task<decimal> FeedRateHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

        public Task<decimal> InitialTargetPositionHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        public Task<decimal> RecoveryTargetPositionHM => this.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

        #endregion
    }
}
