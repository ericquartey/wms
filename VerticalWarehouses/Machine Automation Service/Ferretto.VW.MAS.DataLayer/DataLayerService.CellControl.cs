using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ICellControlDataLayer
    {
        #region Properties

        public decimal FeedRateCC => this.GetDecimalConfigurationValue(CellControl.FeedRate, ConfigurationCategory.CellControl);

        public decimal StepValueCC => this.GetDecimalConfigurationValue(CellControl.StepValue, ConfigurationCategory.CellControl);

        #endregion
    }
}
