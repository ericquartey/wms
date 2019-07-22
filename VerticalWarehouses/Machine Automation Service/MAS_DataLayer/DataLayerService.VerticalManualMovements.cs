using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : IVerticalManualMovements
    {
        #region Properties

        public Task<decimal> FeedRateVM => this.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

        public Task<decimal> InitialTargetPositionVM => this.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);

        public Task<decimal> RecoveryTargetPositionVM => this.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);

        #endregion
    }
}
