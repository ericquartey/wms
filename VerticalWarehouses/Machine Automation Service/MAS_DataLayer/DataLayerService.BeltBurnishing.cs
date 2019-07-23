using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    /// <inheritdoc/>
    public partial class DataLayerService : IBeltBurnishingDataLayer
    {
        #region Properties

        /// <inheritdoc/>
        public int CycleQuantity => this.GetIntegerConfigurationValue((long)BeltBurnishing.CycleQuantity, (long)ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
