using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IBeltBurnishing
    {
        #region Properties

        public int CycleQuantity => this.GetIntegerConfigurationValue((long)BeltBurnishing.CycleQuantity, (long)ConfigurationCategory.BeltBurnishing);

        #endregion
    }
}
