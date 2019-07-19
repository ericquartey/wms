using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    /// <inheritdoc/>
    public partial class DataLayer : IBeltBurnishingDataLayer
    {
        #region Properties

        /// <inheritdoc/>
        public Task<int> CycleQuantity => this.GetIntegerConfigurationValueAsync((long)BeltBurnishing.CycleQuantity, (long)ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
