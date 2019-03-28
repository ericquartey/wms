using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    /// <inheritdoc/>
    public partial class DataLayer : IBeltBurnishing
    {
        #region Properties

        /// <inheritdoc/>
        public Task<int> CycleQuantity => this.GetIntegerConfigurationValueAsync((long)BeltBurnishing.CycleQuantity, (long)ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
