using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    /// <inheritdoc/>
    public partial class DataLayerService : IBeltBurnishingDataLayer
    {
        #region Properties

        /// <inheritdoc/>
        public Task<int> CycleQuantity => this.GetIntegerConfigurationValueAsync((long)BeltBurnishing.CycleQuantity, (long)ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
