using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IBeltBurnishingDataLayer
    {
        #region Properties

        /// <inheritdoc/>
        public int CycleQuantity => this.GetIntegerConfigurationValue((long)BeltBurnishing.CycleQuantity, ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
