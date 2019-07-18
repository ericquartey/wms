using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
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
