using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IDepositAndPickUpDataLayer
    {
        #region Properties

        public int CycleQuantityDP => this.GetIntegerConfigurationValue(DepositAndPickUp.CycleQuantity, ConfigurationCategory.DepositAndPickUp);

        #endregion
    }
}
